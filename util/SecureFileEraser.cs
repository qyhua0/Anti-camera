using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WinDisplay.util
{
    // 安全文件擦除器内部类
    internal class SecureFileEraser
    {
        /// <summary>
        /// 覆盖文件内容并安全删除文件
        /// </summary>
        /// <param name="path">要删除的文件路径</param>
        /// <param name="passes">覆盖次数，默认3次</param>
        /// <param name="bufferSize">缓冲区大小，默认1MB</param>
        /// <param name="finalZeroPass">是否在最后用零覆盖，默认true</param>
        /// <param name="progress">进度回调，参数1:当前已写入字节数，参数2:总字节数</param>
        public static void OverwriteAndDelete(
            string path,
            int passes = 3,
            int bufferSize = 1024 * 1024,
            bool finalZeroPass = true,
            Action<long, long>? progress = null)
        {
            // 参数验证
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("文件未找到", path);
            if (passes < 1) throw new ArgumentOutOfRangeException(nameof(passes));

            // 获取文件所在卷的根目录和文件长度
            string volume = GetVolumeRoot(path); // 例如 "C:\"
            long length = new FileInfo(path).Length;

            string? tempPath = null; // 临时重命名路径

            try
            {
                // 以独占方式打开文件进行覆盖写入
                using (var fs = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.None, // 独占访问，防止其他进程读取
                    bufferSize,
                    FileOptions.WriteThrough | FileOptions.SequentialScan)) // 直写模式和顺序扫描优化
                {
                    // 创建随机数生成器和缓冲区
                    var rng = RandomNumberGenerator.Create();
                    var buffer = new byte[bufferSize];

                    // 执行多次随机数据覆盖
                    for (int pass = 0; pass < passes; pass++)
                    {
                        fs.Seek(0, SeekOrigin.Begin); // 回到文件开头
                        long remaining = length;
                        long writtenThisPass = 0;

                        // 逐块写入随机数据
                        while (remaining > 0)
                        {
                            int toWrite = (int)Math.Min(buffer.Length, remaining);
                            rng.GetBytes(buffer, 0, toWrite); // 生成随机数据
                            fs.Write(buffer, 0, toWrite);
                            remaining -= toWrite;
                            writtenThisPass += toWrite;

                            // 报告进度：当前已写入字节数 + 之前轮次的字节数
                            progress?.Invoke(writtenThisPass + (long)pass * length,
                                           length * passes + (finalZeroPass ? length : 0));
                        }

                        // 强制刷新到磁盘
                        fs.Flush(true);
                        FlushFileBuffers(fs.SafeFileHandle);
                    }

                    // 可选的最终零覆盖（提高安全性）
                    if (finalZeroPass)
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        var zero = new byte[bufferSize]; // 零缓冲区
                        long remaining = length;
                        long writtenZero = 0;

                        // 用零覆盖整个文件
                        while (remaining > 0)
                        {
                            int toWrite = (int)Math.Min(zero.Length, remaining);
                            fs.Write(zero, 0, toWrite);
                            remaining -= toWrite;
                            writtenZero += toWrite;

                            // 报告进度（包含零覆盖阶段）
                            progress?.Invoke(passes * length + writtenZero, length * passes + length);
                        }
                        fs.Flush(true);
                        FlushFileBuffers(fs.SafeFileHandle);
                    }
                }

                // 重命名文件（使其更难恢复）
                var dir = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("无法确定目录");
                tempPath = Path.Combine(dir, Guid.NewGuid().ToString("N")); // 生成随机名称
                File.Move(path, tempPath);

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
        /// 刷新文件缓冲区到磁盘
        /// </summary>
        /// <param name="handle">文件句柄</param>
        private static void FlushFileBuffers(SafeFileHandle handle)
        {
            if (handle == null) return;
            if (handle.IsInvalid) return;
            if (!NativeMethods.FlushFileBuffers(handle))
            {
                // 刷新失败，后续可以增加日志记录
            }
        }

        /// <summary>
        /// 尝试刷新卷缓存
        /// </summary>
        /// <param name="volumeRoot">卷根目录，如"C:\"</param>
        private static void TryFlushVolumeCache(string volumeRoot)
        {
            // 将卷路径转换为设备路径，如"C:\" -> "\\.\C:"
            string devicePath = @"\\.\\" + volumeRoot.TrimEnd('\\') + ":";
            SafeFileHandle? h = null;

            try
            {
                // 打开卷设备
                h = NativeMethods.CreateFile(
                    devicePath,
                    NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    0,
                    IntPtr.Zero);

                // 检查句柄是否有效
                if (h == null || h.IsInvalid)
                {
                    // 无法打开设备，可能没有管理员权限
                    return;
                }

                // 发送刷新缓存IO控制命令
                bool ok = NativeMethods.DeviceIoControl(
                    h,
                    NativeMethods.IOCTL_STORAGE_FLUSH_CACHE,
                    IntPtr.Zero, 0,  // 输入缓冲区和大小
                    IntPtr.Zero, 0,  // 输出缓冲区和大小
                    out int bytesReturned,
                    IntPtr.Zero);

                if (!ok)
                {
                    // 尽力而为，可以记录错误但不抛出异常
                    // 可以使用 Marshal.GetLastWin32Error() 获取错误代码
                }
            }
            finally
            {
                // 确保句柄被关闭
                if (h != null && !h.IsClosed) h.Close();
            }
        }

        // 原生方法封装类
        private static class NativeMethods
        {
            // Windows API 常量定义
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint FILE_SHARE_READ = 0x00000001;
            public const uint FILE_SHARE_WRITE = 0x00000002;
            public const uint OPEN_EXISTING = 3;

            // 存储设备刷新缓存IO控制代码
            public const uint IOCTL_STORAGE_FLUSH_CACHE = 0x002D4808;

            // 创建文件/设备句柄
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern SafeFileHandle CreateFile(
                string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            // 设备IO控制
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool DeviceIoControl(
                SafeFileHandle hDevice,
                uint dwIoControlCode,
                IntPtr lpInBuffer,
                int nInBufferSize,
                IntPtr lpOutBuffer,
                int nOutBufferSize,
                out int lpBytesReturned,
                IntPtr lpOverlapped);

            // 刷新文件缓冲区
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FlushFileBuffers(SafeFileHandle hFile);
        }
    }
}
