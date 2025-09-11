using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDisplay.util
{
    internal class SystemTools
    {

        // 激活当前窗体
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;


        /// <summary>
        /// 检查指定进程是否存在且主窗口标题包含关键词
        /// </summary>
        public static bool IsProcessRunningWithWindowTitle(string processName, string titleKeyword)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    if (process.MainWindowTitle.Contains(titleKeyword))
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException) { /* 忽略已退出的进程 */ }
            }
            return false;
        }



        /// <summary>
        /// 安全启动windows系统工具或可执行文件
        /// </summary>
        /// <param name="program">程序名或完整路径，如 notepad.exe 或 C:\Windows\System32\notepad.exe</param>
        /// <param name="displayName">显示给用户的程序名称（用于错误提示）</param>
        public static void StartSystemTool(string program, string displayName)
        {

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = program,
                    UseShellExecute = true, // 让 shell 处理执行，更安全，支持 .exe/.msc/.bat 等
                    CreateNoWindow = false
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // 使用 UI 提示用户，而不是只写入 Console
                MessageBox.Show(
                    $"无法启动 {displayName}。请检查您的系统环境或权限设置。\n\n错误信息：{ex.Message}",
                    "启动失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }




        /// <summary>
        /// 查找指定进程且主窗口标题包含关键词的实例，若存在则激活其窗口
        /// </summary>
        /// <param name="processName">进程名（如 mmc）</param>
        /// <param name="titleKeyword">窗口标题包含的关键词（如 事件查看器）</param>
        /// <returns>是否成功激活</returns>
        public static bool ActivateWindowIfRunning(string processName, string titleKeyword)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    // 确保进程未退出
                    if (process.HasExited) continue;

                    if (process.MainWindowTitle.Contains(titleKeyword))
                    {
                        IntPtr hWnd = process.MainWindowHandle;

                        // 可能需要先刷新以获取有效句柄
                        if (hWnd == IntPtr.Zero)
                        {
                            process.Refresh();
                            hWnd = process.MainWindowHandle;
                        }

                        if (hWnd != IntPtr.Zero)
                        {
                            // 恢复最小化窗口，并置顶激活
                            ShowWindow(hWnd, SW_RESTORE);
                            SetForegroundWindow(hWnd);
                            return true;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // 忽略因进程已退出导致的异常
                    continue;
                }
            }
            return false; // 未找到匹配的运行实例
        }




        /// <summary>
        /// 启动第三方exe，若已运行则激活其主窗口
        /// </summary>
        /// <param name="exePath">exe文件路径</param>
        /// <param name="processName">进程名（不含.exe）</param>
        /// <param name="arguments">命令行参数（可选）</param>
        /// <returns>是否成功启动或已激活</returns>
        public static bool StartOrActivateProcess(string exePath, string processName, string arguments = "")
        {
            if (!File.Exists(exePath))
            {
                MessageBox.Show($"程序未找到：{exePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 检查进程是否已存在
            var existingProcesses = Process.GetProcessesByName(processName);
            if (existingProcesses.Length > 0)
            {
                // 激活第一个找到的实例
                IntPtr hWnd = existingProcesses[0].MainWindowHandle;

                if (hWnd != IntPtr.Zero)
                {
                    ShowWindow(hWnd, SW_RESTORE);  // 恢复最小化窗口
                    SetForegroundWindow(hWnd);     // 激活并置顶
                }
                else
                {
                    // 窗口句柄可能还未创建，可以尝试刷新或稍等
                    existingProcesses[0].Refresh();
                    hWnd = existingProcesses[0].MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                    }
                    else
                    {
                        // 仍无法获取句柄，放弃激活
                        MessageBox.Show("目标程序已在运行，但无法激活窗口。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                return true;
            }

            // 进程未运行，启动新实例
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,  // 注意：激活窗口时建议设为true，否则可能无法正确显示GUI
                    CreateNoWindow = false   // GUI程序应设为false
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动程序失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }





    }
}
