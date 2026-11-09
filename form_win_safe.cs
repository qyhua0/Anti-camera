/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_win_safe.cs
 * 作者：zhenhua
 * 说明：「安全功能面板」模块：一键打开系统自带的防御工具（任务管理器、Resmon、计算机管理、
 * Autoruns、Process Explorer 等）并检查可疑开机项；所有结果写回界面，不再用看不见的 Console.WriteLine。
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinDisplay.Ui;
using WinDisplay.util;

namespace WinDisplay
{
    /// <summary>
    /// 「安全功能」模块窗体：本身不做任何分析，只负责把系统自带的排查工具（事件查看器、
    /// 资源监视器、任务管理器、计算机管理、本地用户和组、Autoruns、Process Explorer）
    /// 一键拉开，并把每一步成败写回界面。开机项与进程的深挖交给 Autoruns / Process Explorer，
    /// 这里不重复实现一遍。
    /// </summary>
    public partial class form_win_safe : Form
    {
        /// <summary>构造：铺完控件就标定权限胶囊，进面板第一眼看得到当前权限。</summary>
        public form_win_safe()
        {
            InitializeComponent();
            ApplyAdminPill();
        }

        /// <summary>
        /// 权限胶囊：这些管理工具（计算机管理、本地用户和组、Autoruns）都要管理员权限，
        /// 用普通权限跑起来会缺功能，先说清楚比让人瞎试强。
        /// </summary>
        private void ApplyAdminPill()
        {
            if (Program.IsAdminProcess)
            {
                pillAdmin.SetState("管理员权限", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }
            else
            {
                pillAdmin.SetState("普通权限", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
                toolTips.SetToolTip(pillAdmin, "当前不是管理员：这些工具会弹 UAC 让你临时提权；\n如果取消，就只能看到受限的信息。");
            }
        }

        /// <summary>
        /// 把结果写在界面上。原来这些提示全用 Console.WriteLine，
        /// 而这是个 WinForms 程序，没有控制台，出错时界面上一个字都不显示。
        /// </summary>
        private void Report(string message, bool ok = true)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = ok ? UiTheme.TextSecondary : UiTheme.DangerText;
        }

        /// <summary>「事件查看器」按钮：system32\eventvwr.exe，已在跑就切窗口。</summary>
        private void but_evn_Click(object sender, EventArgs e)
        {
            // 已经在跑就切过去，别开第二个事件查看器
            if (SystemTools.ActivateWindowIfRunning("mmc", "事件查看器"))
            {
                Report("事件查看器已经在运行，已切换到它的窗口。");
                return;
            }

            string program = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "eventvwr.exe");
            bool started = SystemTools.StartSystemTool(program, "事件查看器");
            Report(started ? "已打开事件查看器。" : "事件查看器没能打开，详见弹窗。", ok: started);
        }

        /// <summary>「资源监视器」按钮：system32\resmon.exe，看每进程磁盘/网络/活动进程的首选入口。</summary>
        private void but_res_mon_Click(object sender, EventArgs e)
        {
            // 进程名是 Resmon，原来写成 perfmon，永远匹配不上，点一次就多开一个
            if (SystemTools.ActivateWindowIfRunning("Resmon", "资源监视器"))
            {
                Report("资源监视器已经在运行，已切换到它的窗口。");
                return;
            }

            string program = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "resmon.exe");
            bool started = SystemTools.StartSystemTool(program, "资源监视器");
            Report(started ? "已打开资源监视器。" : "资源监视器没能打开，详见弹窗。", ok: started);
        }

        /// <summary>
        /// 「任务管理器」按钮：直接 Process.Start，不提要求也不写 runas，
        /// taskmgr 自己会处理那套 UAC / 「更多信息」提权交互。
        /// </summary>
        private void but_task_Click(object sender, EventArgs e)
        {
            try
            {
                // using：Process 拿着子进程内核句柄，不释放就是每点一次漏一个
                using Process? taskmgr = Process.Start("taskmgr.exe");
                Report("已打开任务管理器。");
            }
            catch (Exception ex)
            {
                Program.WriteLog("启动任务管理器失败", ex);
                Report("启动任务管理器失败：" + ex.Message, ok: false);
            }
        }

        /// <summary>「本地用户和组」按钮：家庭版没有这个单元，失败路径在 <see cref="OpenLocalUsersAndGroups"/> 里。</summary>
        private void but_user_Click(object sender, EventArgs e)
        {
            try
            {
                OpenLocalUsersAndGroups();
            }
            catch (Exception ex)
            {
                Program.WriteLog("打开本地用户和组管理器失败", ex);
                Report("无法打开本地用户和组管理器：" + ex.Message, ok: false);
            }
        }

        /// <summary>「计算机管理」按钮：服务、计划任务、共享、本地用户都在这个控制台里。</summary>
        private void but_computer_Click(object sender, EventArgs e)
        {
            try
            {
                OpenComputerManagement();
            }
            catch (Exception ex)
            {
                Program.WriteLog("打开计算机管理失败", ex);
                Report("无法打开计算机管理：" + ex.Message, ok: false);
            }
        }

        /// <summary>
        /// 打开 compmgmt.msc。.msc 必须 <c>UseShellExecute = true</c>（它是文件关联，不是直接可执行），
        /// 加上 <c>Verb = "runas"</c> 才能看到服务/计划任务的完整节点。
        /// </summary>
        void OpenComputerManagement()
        {
            string fullPath = Path.Combine(Environment.SystemDirectory, "compmgmt.msc");
            if (!File.Exists(fullPath))
            {
                Report($"这台系统里没有 {fullPath}。", ok: false);
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true,   // 打开 .msc 必须走 ShellExecute
                    Verb = "runas"            // 提权，否则很多节点是空的
                };

                using Process? compMgmt = Process.Start(startInfo);
                Report("已打开计算机管理。");
            }
            catch (System.ComponentModel.Win32Exception ex) when ((uint)ex.NativeErrorCode == 1223)
            {
                Report("你取消了管理员授权，计算机管理没有打开。", ok: false);
            }
        }

        /// <summary>打开 lusrmgr.msc；这台系统没有（家庭版）就降级开 ms-settings:accounts，不让按钮点了没反应。</summary>
        void OpenLocalUsersAndGroups()
        {
            string fullPath = Path.Combine(Environment.SystemDirectory, "lusrmgr.msc");
            if (!File.Exists(fullPath))
            {
                // 家庭版没有 lusrmgr.msc，转去现代设置页，别让按钮哑掉
                Report("这台系统（可能是家庭版）没有「本地用户和组」，已改开系统设置里的账户页。");
                OpenModernAccountsSettings();
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using Process? lusrMgr = Process.Start(startInfo);
                Report("已打开本地用户和组管理器。");
            }
            catch (System.ComponentModel.Win32Exception ex) when ((uint)ex.NativeErrorCode == 1223)
            {
                Report("你取消了管理员授权，本地用户和组没有打开。", ok: false);
            }
        }

        /// <summary>家庭版降级入口：用 ms-settings 协议打开「设置 - 账户」。</summary>
        void OpenModernAccountsSettings()
        {
            try
            {
                using Process? settings = Process.Start(new ProcessStartInfo("ms-settings:accounts") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Program.WriteLog("打开账户设置失败", ex);
                Report("无法打开账户设置：" + ex.Message, ok: false);
            }
        }

        /// <summary>
        /// 「启动项完整清单」：带在 third_party\ 下的绿色版 Autoruns64.exe。
        /// 它不是本项目编译产物，没带进来（或杀软删了）就是打不开，所以失败也要写回界面。
        /// </summary>
        private void but_autoruns_Click(object sender, EventArgs e)
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "third_party", "Autoruns64.exe");
            if (SystemTools.StartOrActivateProcess(exePath, "Autoruns64"))
            {
                Report("已打开 Autoruns（启动项完整清单）。");
            }
            else
            {
                Report("没找到 third_party\\Autoruns64.exe，启动项清单打不开。", ok: false);
            }
        }

        /// <summary>「进程树细查」：绿色版 Process Explorer（procexp64.exe），同样是 third_party\ 下的外部工具。</summary>
        private void but_procexp_Click(object sender, EventArgs e)
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "third_party", "procexp64.exe");
            if (SystemTools.StartOrActivateProcess(exePath, "procexp64"))
            {
                Report("已打开 Process Explorer。");
            }
            else
            {
                Report("没找到 third_party\\procexp64.exe，Process Explorer 打不开。", ok: false);
            }
        }
    }
}
