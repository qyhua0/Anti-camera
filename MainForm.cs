/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：MainForm.cs
 * 作者：zhenhua
 * 说明：主窗体逻辑：左侧模块导航 + 右侧内容宿主 + 底部状态条。
 * 模块窗体懒加载（第一次点进去才构造），切走只隐藏不销毁，退出时统一 Close。
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinDisplay.Ui;

namespace WinDisplay
{
    /// <summary>
    /// 主窗体：左侧模块导航 + 右侧内容宿主 + 底部状态条。
    /// 模块窗体懒加载（第一次点进去才构造），切走只隐藏不销毁，退出时统一 Close。
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Form> _modules = new(StringComparer.Ordinal);
        private readonly ToolTip _tip = new() { AutoPopDelay = 12000, InitialDelay = 300 };
        private string _currentTag = string.Empty;

        // 托盘归属：v2.0 把各模块窗体改成嵌在 contentHost 里的子窗体，顶层窗口只剩主窗体一个。
        // 托盘图标显示/隐藏、还原窗口都是顶层窗口的动作，所以只能由主窗体来做（旧版是「屏幕防拍摄」
        // 模块自己持有托盘，见 frm_screen 里删掉的 InitializeSystemTray）。
        private NotifyIcon? _tray;
        private ContextMenuStrip? _trayMenu;
        private bool _exitRequested;   // 只有托盘菜单「退出」置位，用来区分「关窗 = 收进托盘」和「真的退出」
        private bool _trayHintShown;   // 第一次收进托盘时说明一次，之后不再唠叨

        /// <summary>当前主窗体。程序是单实例，一个静态引用足够；托盘唤醒和跨进程唤醒都要靠它。</summary>
        internal static MainForm? Instance { get; private set; }

        /// <summary>模块的说明文字，显示在底部状态条，比导航里那行更具体。</summary>
        private static readonly Dictionary<string, string> ModuleHints = new(StringComparer.Ordinal)
        {
            ["screen"] = "滑块调暗屏幕；「添加挡板」会盖住整个屏幕，托盘图标可恢复亮度。",
            ["net"] = "抓包需要已安装 Npcap，并以管理员身份运行本程序。",
            ["safe"] = "系统工具与第三方工具，部分需要管理员权限。",
            ["clear"] = "先扫描再清理；只删扫描列出的那几类文件，只读项不会被动。",
            ["remove"] = "粉碎后的文件无法恢复，请确认路径再执行。",
        };

        public MainForm()
        {
            // 必须在构造任何子窗体之前设置，否则各窗体还是老的 Microsoft Sans Serif
            UiTheme.ApplyDefaultFont();
            InitializeComponent();

            navModules.Add("屏幕防拍摄", "screen", "遮罩 · 亮度 · 高频闪烁");
            navModules.Add("网络流量分析", "net", "实时连接与进程流量");
            navModules.Add("安全功能面板", "safe", "系统与第三方安全工具");
            navModules.Add("垃圾清理", "clear", "扫描并清理临时文件");
            navModules.Add("文件粉碎", "remove", "不可恢复地删除文件");
            navModules.SelectedIndexChanged += NavModules_SelectedIndexChanged;

            ApplyAdminPill();
            lblVersion.Text = "版本 " + AppVersion;
            _tip.SetToolTip(lnkLog, "日志目录：" + Path.GetDirectoryName(Program.LogFilePath));

            Instance = this;
            InitializeTray();

            navModules.SelectedIndex = 0;
        }

        /// <summary>版本号取程序集，避免和 csproj 里写的数字各说各话。</summary>
        private static string AppVersion
        {
            get
            {
                Version? v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return v == null ? "未知" : $"{v.Major}.{v.Minor}";
            }
        }

        private void ApplyAdminPill()
        {
            bool admin = Program.IsAdminProcess;
            if (admin)
            {
                pillAdmin.SetState("管理员权限", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
                _tip.SetToolTip(pillAdmin, "当前以管理员身份运行，清理、提权、抓包等功能可用。");
            }
            else
            {
                pillAdmin.SetState("普通权限", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
                _tip.SetToolTip(pillAdmin, "当前不是管理员：清理系统目录、抓包、打开系统工具会失败。需要时请右键「以管理员身份运行」。");
            }
        }

        private void NavModules_SelectedIndexChanged(object? sender, EventArgs e)
        {
            NavList.NavItem? item = navModules.SelectedItem;
            if (item?.Tag is string tag)
            {
                ShowModule(tag);
            }
        }

        /// <summary>切到某个模块：没有就建，建过就只显示这一个。</summary>
        private void ShowModule(string tag)
        {
            if (!_modules.TryGetValue(tag, out Form? form))
            {
                try
                {
                    form = CreateModule(tag);
                }
                catch (Exception ex)
                {
                    ShowError("模块打开失败", ex);
                    return;
                }

                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                _modules[tag] = form;
                contentHost.Controls.Add(form);
            }

            foreach (Form other in _modules.Values)
            {
                other.Visible = ReferenceEquals(other, form);
            }

            form.Visible = true;
            form.BringToFront();
            _currentTag = tag;
            SetHint(ModuleHints.TryGetValue(tag, out string? hint) ? hint : string.Empty);
        }

        private static Form CreateModule(string tag) => tag switch
        {
            "screen" => new frm_screen(),
            "net" => new Form_net(),
            "safe" => new form_win_safe(),
            "clear" => new form_clear(),
            "remove" => new form_safe_remove(),
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, "未知模块")
        };

        private void SetHint(string text) => lblHint.Text = text;

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && !e.Alt && e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D5)
            {
                navModules.SelectedIndex = e.KeyCode - Keys.D1;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LnkLog_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            string log = Program.LogFilePath;
            string? dir = Path.GetDirectoryName(log);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                SetHint("还没有产生错误日志：" + log);
                return;
            }
            try
            {
                // 有日志文件就把它选中，没有就只打开目录
                using Process? p = File.Exists(log)
                    ? Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{log}\"") { UseShellExecute = true })
                    : Process.Start(new ProcessStartInfo("explorer.exe", $"\"{dir}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError("打开日志目录失败", ex);
            }
        }

        private void ShowError(string title, Exception ex)
        {
            Program.WriteLog(title, ex);
            MessageBox.Show(this,
                ex.Message + Environment.NewLine + Environment.NewLine + "详情已写入日志：" + Program.LogFilePath,
                title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #region 系统托盘

        /// <summary>
        /// 建托盘图标 + 右键菜单。图标用主窗体自己的图标（MainForm.resx 里的 $this.Icon），
        /// 不再依赖模块窗体的资源。
        /// </summary>
        private void InitializeTray()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("显示窗口", null, (s, e) => RestoreFromTray());
            // 兜底逃生口：屏幕被压得很暗时，界面看不清，托盘是唯一还点得到的地方（减光遮罩鼠标穿透）
            _trayMenu.Items.Add("恢复亮度", null, (s, e) => RestoreBrightnessFromTray());
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("退出", null, (s, e) => ExitApplication());

            _tray = new NotifyIcon
            {
                Icon = Icon ?? SystemIcons.Application,
                Text = "x-保护 · 屏幕防拍摄",
                ContextMenuStrip = _trayMenu,
                Visible = true
            };
            // 单击也恢复：以前只绑了 DoubleClick，用户点一下没反应，会以为程序挂了（托盘就这么大一个点）
            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) RestoreFromTray();
            };
        }

        /// <summary>
        /// 把收进托盘的主窗口叫回来。要点：必须先让顶层窗口本身可见/还原，再管模块——
        /// 模块是子窗体，主窗口没显示时把它 Visible=true 也什么都看不见，这就是「恢复不了窗口」的根因。
        /// </summary>
        internal void RestoreFromTray()
        {
            if (IsDisposed) return;

            // 唤醒请求可能来自监听线程（见 Program.StartShowRequestListener），一律回 UI 线程操作窗口
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(RestoreFromTray)); } catch { }
                return;
            }

            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;

            // 模块的可见性只在 ShowModule 里维护；被别处 Hide() 过的话这里补一刀，保证当前模块跟着出来
            if (_modules.TryGetValue(_currentTag, out Form? current) && !current.IsDisposed && !current.Visible)
            {
                current.Visible = true;
                current.BringToFront();
            }

            BringToFront();
            Activate();
            // 前台切换有系统限制（不是前台进程时 Activate 会被挡掉），借一次 TopMost 抖动把它顶上来
            if (!Focused)
            {
                TopMost = true;
                TopMost = false;
                Activate();
            }
        }

        /// <summary>托盘「恢复亮度」：屏幕模块没建过就说明伽马从没动过，直接忽略。</summary>
        private void RestoreBrightnessFromTray()
        {
            if (_modules.TryGetValue("screen", out Form? form) && form is frm_screen screen && !screen.IsDisposed)
            {
                screen.RestoreBrightness();
            }
        }

        /// <summary>托盘气泡提示。屏幕很暗、界面看不见的提示也只能走这里。</summary>
        internal void ShowBalloon(string title, string text)
        {
            try { _tray?.ShowBalloonTip(6000, title, text, ToolTipIcon.Info); } catch { }
        }

        /// <summary>托盘「退出」：置位后再走正常关窗流程，各模块的清理（复位亮度、销毁遮罩）才会跑到。</summary>
        private void ExitApplication()
        {
            _exitRequested = true;
            Application.Exit();
        }

        /// <summary>用户点 X = 收进托盘继续防护（遮罩/减光是有用的，关窗不该把它们一起带走）。</summary>
        private void HideToTray()
        {
            // 人在离开状态下不该还能拖动/关掉挡板，沿用旧版「最小化时锁挡板」的行为
            if (_modules.TryGetValue("screen", out Form? form) && form is frm_screen screen && !screen.IsDisposed)
            {
                screen.LockMasksForTray();
            }

            Hide();

            if (!_trayHintShown)
            {
                _trayHintShown = true;
                ShowBalloon("已在托盘运行", "双击（或单击）托盘图标恢复窗口；右键「退出」才会真正退出并恢复屏幕亮度。");
            }
        }

        /// <summary>
        /// 只拦用户点 X（CloseReason.UserClosing）。系统关机/注销、Application.Exit 都必须放行，
        /// 否则关机时亮度和遮罩的清理就跑不到了。
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_exitRequested && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideToTray();
            }

            base.OnFormClosing(e);
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 托盘图标必须在关窗时显式隐藏并释放：只 Dispose 不设 Visible=false，图标会在托盘里
            // 留一个「幽灵」，直到鼠标划过去才消失（Explorer 的已知行为）
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
            _trayMenu?.Dispose();
            _trayMenu = null;
            if (ReferenceEquals(Instance, this)) Instance = null;

            // 各模块自己关窗时才会跑它们的清理（复位亮度、销毁遮罩、停抓包），所以先 Close 再兜底 Dispose
            foreach (Form module in _modules.Values)
            {
                try { module.Close(); } catch { }
                try { module.Dispose(); } catch { }
            }
            _modules.Clear();
            _tip.Dispose();
            base.OnFormClosed(e);
        }
    }
}
