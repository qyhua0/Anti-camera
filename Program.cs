using System.Runtime.InteropServices;

namespace WinDisplay
{
    internal static class Program
    {

        // 导入 Win32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        // 导入 FindWindow 方法
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "Global\\BethVgs7KY8McUFx0dDdUsD6Fl2v1lc1GG2fhvtgDT4HZAmuoZ2vKrvzb4kqpgfj";
            bool createdNew;

            using (Mutex mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (createdNew)
                {
                 
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                }
                else
                {
                    // 已有实例，尝试激活窗口
                    IntPtr hWnd = FindWindow(null, "YourFormTitle"); // 替换为你的窗体标题
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE); // 还原窗口（如果最小化）
                        SetForegroundWindow(hWnd); // 激活窗口
                    }
                    else
                    {
                        MessageBox.Show("程序已在运行，无法启动第二个实例！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }
            }
        }
    }
}