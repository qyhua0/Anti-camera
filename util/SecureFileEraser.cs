/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：util/SecureFileEraser.cs
 * 作者：zhenhua
 * 说明：不可恢复删除：多次全量覆写 + 改名 + 真删除（绕过回收站），并处理只读属性与占用中的文件。
 * 只擦数据区，不承诺擦掉磁盘上的 slack/重映射扇区——那是全盘擦除工具的事。
 */
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WinDisplay.util
{
    /// <summary>
    /// 安全文件擦除：随机数据覆写 N 遍 → 改名 → 删除。
    /// <para>
    /// 诚实的边界：SSD 的磨损均衡/预留空间、机械盘的坏道重映射、以及文件系统日志都意味着
    /// 软件覆写不能保证原始数据一定从介质上消失。这个类只是大幅提高恢复门槛，不是保密手段。
    /// </para>
    /// </summary>
    internal class SecureFileEraser
    {
        public const int MinPasses = 1;
        public const int MaxPasses = 32;
        private const int MinBufferSize = 4 * 1024;
        private const int MaxBufferSize = 64 * 1024 * 1024;

        /// <summary>
        /// 覆盖文件内容并安全删除文件
        /// </summary>
        /// <param name="path">要删除的文件路径</param>
        /// <param name="passes">覆盖次数，默认3次</param>
        /// <param name="bufferSize">缓冲区大小，默认1MB</param>
        /// <param name="finalZeroPass">是否在最后用零覆盖，默认true</param>
        /// <param name="progress">进度回调，参数1:当前已写入字节数，参数2:总字节数。回调内异常会被吞掉</param>
        /// <exception cref="ArgumentException">路径为空</exception>
        /// <exception cref="ArgumentOutOfRangeException">覆盖次数或缓冲区大小越界</exception>
        /// <exception cref="FileNotFoundException">文件不存在</exception>
        public static void OverwriteAndDelete(
            string path,
            int passes = 3,
            int bufferSize = 1024 * 1024,
            bool finalZeroPass = true,
            Action<long, long>? progress = null)
        {
            // 参数验证。原来只挡了 passes < 1，界面却允许填到 1000 遍（每遍重写整个文件）
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("文件路径不能为空", nameof(path));
            if (passes < MinPasses || passes > MaxPasses)
                throw new ArgumentOutOfRangeException(nameof(passes),
                    $"覆盖次数必须在 {MinPasses}-{MaxPasses} 之间，收到 {passes}");
            if (bufferSize < MinBufferSize || bufferSize > MaxBufferSize)
                throw new ArgumentOutOfRangeException(nameof(bufferSize),
                    $"缓冲区大小必须在 {MinBufferSize}-{MaxBufferSize} 字节之间，收到 {bufferSize}");

            string fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath)) throw new FileNotFoundException("文件未找到", fullPath);

            // 获取文件所在卷的根目录和文件长度
            string volume = GetVolumeRoot(fullPath); // 例如 "C:\"
            long length = new FileInfo(fullPath).Length;

            // 总进度：随机覆写 passes 遍 + 可选的最后一遍零覆盖
            long totalBytes = length > 0 ? length * passes + (finalZeroPass ? length : 0) : 0;
            long doneBytes = 0;

            string? tempPath = null; // 临时重命名路径

            try
            {
                // 只读/隐藏/系统属性会让「以写方式打开」直接失败，先清掉
                try { File.SetAttributes(fullPath, FileAttributes.Normal); }
                catch { /* 没权限改属性，就让它在校验位置抛出真正的异常 */ }

                // 以独占方式打开文件进行覆盖写入
                using (var fs = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.None, // 独占访问，防止其他进程读取
                    bufferSize,
                    FileOptions.WriteThrough | FileOptions.SequentialScan)) // 直写模式和顺序扫描优化
                using (var rng = RandomNumberGenerator.Create())
                {
                    var buffer = new byte[bufferSize];

                    // 执行多次随机数据覆盖
                    for (int pass = 0; pass < passes && length > 0; pass++)
                    {
                        fs.Seek(0, SeekOrigin.Begin); // 回到文件开头
                        long remaining = length;

                        // 逐块写入随机数据
                        while (remaining > 0)
                        {
                            int toWrite = (int)Math.Min(buffer.Length, remaining);
                            rng.GetBytes(buffer, 0, toWrite); // 生成随机数据
                            fs.Write(buffer, 0, toWrite);
                            remaining -= toWrite;
                            doneBytes += toWrite;

                            ReportProgress(progress, doneBytes, totalBytes);
                        }

                        // 强制刷新到磁盘（FileStream.Flush(true) 内部就是 FlushFileBuffers）
                        fs.Flush(true);
                    }

                    // 可选的最终零覆盖（提高安全性）
                    if (finalZeroPass && length > 0)
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        Array.Clear(buffer, 0, buffer.Length); // 复用同一个缓冲区，不再另开 1MB
                        long remaining = length;

                        while (remaining > 0)
                        {
                            int toWrite = (int)Math.Min(buffer.Length, remaining);
                            fs.Write(buffer, 0, toWrite);
                            remaining -= toWrite;
                            doneBytes += toWrite;

                            ReportProgress(progress, doneBytes, totalBytes);
                        }

                        fs.Flush(true);
                    }
                }

                // 重命名文件（使文件名也不留在目录项里）
                var dir = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("无法确定目录");
                tempPath = Path.Combine(dir, Guid.NewGuid().ToString("N"));
                File.Move(fullPath, tempPath);

                // 刷新卷缓存
                TryFlushVolumeCache(volume);

                // 删除重命名后的文件
                File.Delete(tempPath);
                tempPath = null;

                // 再次刷新卷缓存确保数据完全清除
                TryFlushVolumeCache(volume);
            }
            catch
            {
                // 发生异常时尝试清理临时文件
                try { if (tempPath != null && File.Exists(tempPath)) File.Delete(tempPath); }
                catch { /* 忽略清理异常 */ }
                throw; // 重新抛出原始异常
            }
        }

        private static void ReportProgress(Action<long, long>? progress, long done, long total)
        {
            if (progress == null) return;
            try { progress(done, total); }
            catch { /* 进度回调一般跑在 UI 上，它出错不该打断擦除 */ }
        }

        /// <summary>
        /// 获取文件所在卷的根目录
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>卷根目录，如"C:\"</returns>
        private static string GetVolumeRoot(string path)
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (string.IsNullOrEmpty(root)) throw new InvalidOperationException("无法确定卷根目录");
            return root;
        }

        /// <summary>
        /// 尝试刷新整卷缓存。做法是按微软文档打开卷句柄（需要写权限）后调 FlushFileBuffers。
        /// <para>
        /// 这里原来发的是 <c>DeviceIoControl(h, 0x002D4808, ...)</c> 并注释成「刷新缓存IO控制命令」。
        /// 0x002D4808 = CTL_CODE(FILE_DEVICE_STORAGE=0x2D, 0x202, METHOD_BUFFERED, FILE_ANY_ACCESS)
        /// = <b>IOCTL_STORAGE_EJECT_MEDIA（弹出介质）</b>，跟刷新缓存毫无关系：
        /// 对 U 盘/读卡器执行文件粉碎时，会先把用户的盘给「出盘」。
        /// 弹盘是写操作且不可逆，绝不能再出现在这条路径上。
        /// </para>
        /// </summary>
        /// <param name="volumeRoot">卷根目录，如"C:\"</param>
        private static void TryFlushVolumeCache(string volumeRoot)
        {
            // 只处理盘符形式的根（"C:\"）。UNC 共享、\\?\ 卷 GUID 之类拼不出设备路径，直接跳过
            if (volumeRoot.Length != 3 || volumeRoot[1] != ':' || volumeRoot[2] != '\\') return;

            // 将卷路径转换为设备路径，如"C:\" -> "\\.\C:"
            string devicePath = @"\\.\" + volumeRoot.TrimEnd('\\') + ":";

            try
            {
                using SafeFileHandle h = NativeMethods.CreateFile(
                    devicePath,
                    NativeMethods.GENERIC_WRITE,   // FlushFileBuffers 只要求写权限，不需要读权限
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    0,
                    IntPtr.Zero);

                if (h == null || h.IsInvalid)
                {
                    // 打不开卷设备：普通用户对固定磁盘没有写权限（ERROR_ACCESS_DENIED），属正常情况。
                    // 文件句柄上的 Flush(true) 已经把本次写入落盘，这里只是尽力而为地顺带刷整卷。
                    return;
                }

                if (!NativeMethods.FlushFileBuffers(h))
                {
                    // 尽力而为，不抛异常（可用 Marshal.GetLastWin32Error() 排查）
                }
            }
            catch
            {
                // 刷新卷缓存失败不影响「文件已被覆写并删除」这个事实
            }
        }

        // 原生方法封装类
        private static class NativeMethods
        {
            // Windows API 常量定义
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint FILE_SHARE_READ = 0x00000001;
            public const uint FILE_SHARE_WRITE = 0x00000002;
            public const uint OPEN_EXISTING = 3;

            // 创建文件/设备句柄
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern SafeFileHandle CreateFile(
                string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            // 刷新文件/卷缓冲区。对卷句柄调用即刷新整卷缓存
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FlushFileBuffers(SafeFileHandle hFile);
        }
    }
}
