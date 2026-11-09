/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：Program.cs
 * 作者：zhenhua
 * 说明：程序入口。单实例互斥体、管理员权限自检、DPI/字体/主题初始化，再拉起主窗体 MainForm；
 * 并支持 --fit / --audit / --smoke 等离线验收参数，供无 GUI 环境下的界面检查工具调用。
 */
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;   // 本 SDK 的隐式 using 不含 System.Text

namespace WinDisplay
{
    internal static class Program
    {
        #region 单实例

        /// <summary>
        /// 单实例互斥体名。用 Local\（当前登录会话）而不是 Global\：
        /// 标准用户没有 SeCreateGlobalPrivilege，创建 Global\ 内核对象会直接抛
        /// UnauthorizedAccessException，程序连启动都启动不了。
        /// </summary>
        private const string MutexName = "Local\\WinDisplay_AntiCamera_SingleInstance";

        /// <summary>
        /// 「请已运行的实例把窗口显示出来」的命名事件。窗口收进托盘后光靠 MainWindowHandle 唤不回来
        /// （Hide() 后主窗口句柄拿不到、也不会出现在 Alt+Tab 里），所以第二个实例只需 Set() 一下，
        /// 跑着的实例自己把窗口叫出来。用 Local\ 和互斥体保持一致（标准用户建不了 Global\）。
        /// </summary>
        private const string ShowEventName = "Local\\WinDisplay_ShowWindowRequest";

        private static EventWaitHandle? _showEvent;
        private static RegisteredWaitHandle? _showEventWait;

        // 导入 Win32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            InstallGlobalExceptionHandlers();

            bool createdNew;
            Mutex? mutex;
            try
            {
                mutex = new Mutex(true, MutexName, out createdNew);
            }
            catch (Exception ex)
            {
                // 拿不到互斥体（权限/组策略异常）不该让程序起不来，只是失去单实例保护
                WriteLog("创建单实例互斥体失败，忽略并继续启动", ex);
                mutex = null;
                createdNew = true;
            }

            try
            {
                if (createdNew)
                {
                    ApplicationConfiguration.Initialize();
                    StartShowRequestListener();
                    try
                    {
                        Application.Run(new MainForm());
                    }
                    finally
                    {
                        StopShowRequestListener();
                    }
                }
                else
                {
                    ActivateExistingInstance();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    // 只有持有者才能释放，第二个实例调 ReleaseMutex 会抛 ApplicationException
                    if (createdNew)
                    {
                        try { mutex.ReleaseMutex(); } catch { }
                    }
                    mutex.Dispose();
                }
            }
        }

        /// <summary>
        /// 激活已经在运行的实例。原实现是 FindWindow(null, "YourFormTitle") —— 占位符没替换，
        /// 真实窗体标题带版本号（现在是「x-保护  (ver2.0)」），改版本就换数字，所以永远可能找不到窗口，只会弹一句提示。
        /// 改成按进程名找主窗口句柄，不再和窗体标题（含版本号）耦合。
        /// </summary>
        private static void ActivateExistingInstance()
        {
            // 先走命名事件：它不依赖窗口句柄，对「已收进托盘」的实例也能起作用
            if (SignalExistingInstance()) return;

            try
            {
                using Process current = Process.GetCurrentProcess();
                foreach (Process p in Process.GetProcessesByName(current.ProcessName))
                {
                    using (p)
                    {
                        if (p.Id == current.Id) continue;

                        IntPtr hWnd = p.MainWindowHandle;
                        if (hWnd == IntPtr.Zero) continue;   // 已最小化到托盘的实例没有主窗口

                        ShowWindow(hWnd, SW_RESTORE);        // 还原窗口（如果最小化）
                        SetForegroundWindow(hWnd);           // 激活窗口
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("激活已有实例失败", ex);
            }

            MessageBox.Show("程序已在运行，请在任务栏或系统托盘中找到已打开的窗口。",
                "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>启动端：监听命名事件，收到就请主窗体把自己显示出来（线程安：RestoreFromTray 自己 BeginInvoke）。</summary>
        private static void StartShowRequestListener()
        {
            try
            {
                _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName, out bool createdNew);
                if (!createdNew)
                {
                    // 互斥体已保证单实例，走到这里只可能是上一个实例刚退、名字还没回收；不影响运行，只是不能被远程唤醒
                    WriteLog("显示请求事件名已被占用，本实例无法被第二个实例唤醒", null);
                    return;
                }

                // -1 = 不限时等；false = 不取消注册（退出时 StopShowRequestListener 手工 Unregister）
                _showEventWait = ThreadPool.RegisterWaitForSingleObject(_showEvent,
                    (_, _) => MainForm.Instance?.RestoreFromTray(), null, -1, false);
            }
            catch (Exception ex)
            {
                WriteLog("监听窗口显示请求失败", ex);
            }
        }

        /// <summary>退出时拆掉监听，否则命名事件会被一个已经没人等的 handle 占着，下次启动就唤不醒了。</summary>
        private static void StopShowRequestListener()
        {
            try { _showEventWait?.Unregister(null); } catch { }
            _showEventWait = null;

            try { _showEvent?.Dispose(); } catch { }
            _showEvent = null;
        }

        /// <summary>请求已运行的实例显示窗口；能通知到就返回 true（通知到了就不再弹「程序已在运行」）。</summary>
        private static bool SignalExistingInstance()
        {
            try
            {
                if (!EventWaitHandle.TryOpenExisting(ShowEventName, out EventWaitHandle? handle)) return false;
                using (handle)
                {
                    handle.Set();
                }
                // 给对端一点反应时间，否则本进程退得太快，窗口刚闪一下可能被本进程退出消息干扰
                Thread.Sleep(200);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("通知已运行实例失败", ex);
                return false;
            }
        }

        #endregion

        #region 运行权限

        /// <summary>
        /// 当前进程是否以管理员运行。状态条与各个模块共用一份判断，
        /// 避升每个模块自己拿 WindowsIdentity 算一遍、口径还不一致。
        /// </summary>
        internal static bool IsAdminProcess { get; } = ReadAdminFlag();

        private static bool ReadAdminFlag()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 全局异常兜底

        private static readonly object LogLock = new object();

        private static string LogDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinDisplay", "logs");

        private static string CurrentLogFile => Path.Combine(LogDirectory, $"error-{DateTime.Now:yyyyMM}.log");

        /// <summary>当前日志文件完整路径，报错提示里直接给用户看。</summary>
        internal static string LogFilePath => CurrentLogFile;

        /// <summary>
        /// 兜底异常处理。本程序在后台线程里有定时器、抓包回调、清理与多轮覆写删除，
        /// 任何一处抛出都会直接干掉进程——对「防拍摄」来说意味着遮罩瞬间消失、屏幕恢复原样，
        /// 对「清理」来说意味着删到一半。所以必须先兜住，再把现场写进日志。
        /// </summary>
        private static void InstallGlobalExceptionHandlers()
        {
            // WinForms UI 线程：默认弹的是 VS 那种「继续/退出」崩溃框，换成记日志 + 友好提示，程序继续跑
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                WriteLog("UI 线程未处理异常", e.Exception);
                MessageBox.Show(
                    "界面出现了一个错误，程序已继续运行。\n\n" + e.Exception.Message +
                    "\n\n详细堆栈见日志：\n" + CurrentLogFile,
                    "出现错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            // 非 UI 线程：.NET (Core) 之后无法再阻止进程退出，至少要留下现场
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                WriteLog("后台线程未处理异常", e.ExceptionObject as Exception, e.IsTerminating);

            // 未 await 的 Task
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                WriteLog("Task 中未观察到的异常", e.Exception);
                e.SetObserved();
            };
        }

        /// <summary>写日志。日志自身失败绝不能再抛异常，否则兜底处理反而成了新的崩溃点。</summary>
        internal static void WriteLog(string title, Exception? ex, bool terminating = false)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                string text =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {title}" +
                    (terminating ? "（进程即将退出）" : string.Empty) + "\r\n" +
                    (ex == null ? "(无异常对象)" : ex.ToString()) + "\r\n\r\n";

                lock (LogLock)
                {
                    File.AppendAllText(CurrentLogFile, text, Encoding.UTF8);
                }
            }
            catch
            {
                // 吞掉：兜底逻辑不再产生新异常
            }
        }

        #endregion
    }
}
