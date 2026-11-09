/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：ScreenDimOverlay.cs
 * 作者：zhenhua
 * 说明：全屏半透明遮罩减光（每个显示器一层），补伽马压不下去的那一段。
 * 不碰显卡、靠窗口合成器混色，因此任何机器、任何会话（含远程桌面）都有效。
 */
using System.Windows.Forms;
using Microsoft.Win32;

namespace WinDisplay
{
    /// <summary>
    /// 全屏半透明遮罩减光（每个显示器一层）。
    /// <para>
    /// 为什么需要它：<see cref="ScreenBrightnessControl"/> 走显卡伽马曲线，驱动限制「LUT 相对原亮度
    /// 的偏离量不能超过约一半」，实测本机（Intel Iris Xe）低于 50% 就直接被拒绝，远程桌面里更是完全无效。
    /// 遮罩减光不碰显卡，靠窗口合成器混色，所以任何机器、任何会话（包括远程桌面）都有效，
    /// 而且能压到伽马根本做不到的暗度。
    /// </para>
    /// <para>
    /// 两级配合：总亮度 = 伽马亮度 × 遮罩透光率。<see cref="frm_screen.ApplyBrightness"/> 先用伽马压
    /// 到驱动允许的最暗值，剩下的差额由这里接着压，所以滑到 20% 就真的得到 20%。
    /// </para>
    /// <para>
    /// 关键性质：<b>鼠标穿透</b>（WS_EX_TRANSPARENT）+ <b>不抢焦点</b>（WS_EX_NOACTIVATE）+
    /// <b>不进 Alt-Tab</b>（WS_EX_TOOLWINDOW）。它只是一片“墨镜”，不是挡板：底下的程序照常点得动，
    /// 不然减光和锁屏就没区别了。要挡住内容请另开遮挡窗体（<c>frm_screen.CreateMaskForm</c>）。
    /// </para>
    /// <para>
    /// 只可在 UI 线程调用（内部是 WinForms 窗体）。进程退出时系统会连带销毁这些窗口，
    /// 所以崩溃不会把用户屏幕永久糊住；正常关窗由 <see cref="DisposeAll"/> 负责。
    /// </para>
    /// </summary>
    internal static class ScreenDimOverlay
    {
        /// <summary>
        /// 遮罩最深压到多少透光率。留 10% 是安全垫：全黑等于屏幕内容彻底看不见，
        /// 用户万一想往回拉却一片黑，就只能靠记忆摸滑块了。
        /// </summary>
        public const int MinBrightness = 10;

        private static readonly Dictionary<string, DimForm> Forms = new();
        private static bool subscribed;
        private static int brightnessPercent = 100;

        /// <summary>当前是否正在减光。</summary>
        public static bool IsActive => Forms.Count > 0;

        /// <summary>当前遮罩档位（100 = 未减光）。</summary>
        public static int Brightness => brightnessPercent;

        /// <summary>
        /// 设置遮罩透光率：100 = 完全透明（隐藏），越小越暗，最低 <see cref="MinBrightness"/>。
        /// </summary>
        public static void SetBrightness(int brightnessPercent)
        {
            brightnessPercent = Math.Clamp(brightnessPercent, MinBrightness, 100);
            ScreenDimOverlay.brightnessPercent = brightnessPercent;

            if (brightnessPercent >= 100)
            {
                DisposeAll();
                return;
            }

            EnsureSubscribed();

            // 以 DeviceName 为键：插拔屏/改分辨率时对得上号，多余的下一轮清掉
            var alive = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            double opacity = 1.0 - brightnessPercent / 100.0;

            foreach (Screen screen in Screen.AllScreens)
            {
                alive.Add(screen.DeviceName);
                Rectangle bounds = screen.Bounds;

                if (Forms.TryGetValue(screen.DeviceName, out DimForm? existing) && !existing.IsDisposed)
                {
                    // 只调数值，不重建窗口：重建会闪一下，而且拖滑块时每格都重建
                    if (existing.Bounds != bounds) existing.Bounds = bounds;
                    if (Math.Abs(existing.Opacity - opacity) > 0.001) existing.Opacity = opacity;
                    continue;
                }

                var form = new DimForm(bounds, opacity);
                // 只删“还是我自己”的那一条：这个设备以后可能被重建过一层，
                // 无条件 Remove 会把新的一层从字典里误删，它就变成没人管的幽灵窗口
                form.Disposed += (s, e) =>
                {
                    if (Forms.TryGetValue(screen.DeviceName, out DimForm? cur) && ReferenceEquals(cur, form))
                        Forms.Remove(screen.DeviceName);
                };
                Forms[screen.DeviceName] = form;
                form.Show();
            }

            // 已经拔掉的外接屏上残留的那层要收掉，否则它盖在一个不存在的坐标上
            List<string>? stale = null;
            foreach (string device in Forms.Keys)
            {
                if (!alive.Contains(device)) (stale ??= new List<string>()).Add(device);
            }
            if (stale != null)
            {
                foreach (string device in stale)
                {
                    DimForm dead = Forms[device];
                    Forms.Remove(device);
                    dead.Dispose();
                }
            }
        }

        /// <summary>
        /// 把遮罩重新顶到最上层。遮挡窗体也是 TopMost 且会激活，后显示的会盖到墨镜上面去；
        /// 每开一个遮挡窗体之后调一次，保证「减光」仍然作用在遮挡窗体上。
        /// </summary>
        public static void ReassertTopMost()
        {
            foreach (DimForm form in Forms.Values)
            {
                if (!form.IsDisposed) form.PushTopMost();
            }
        }

        /// <summary>撤掉全部遮罩层（不销毁订阅，留着下次用）。</summary>
        public static void HideAll() => SetBrightness(100);

        /// <summary>关窗/退出时调用：销毁窗体并退订显示设置变化事件。</summary>
        public static void DisposeAll()
        {
            // 先抓快照再 Dispose：Dispose 会触发 Disposed 处理器往 Forms 里 Remove，
            // 直接边遍历边销毁会抛 InvalidOperationException（集合已修改）
            var snapshot = new List<DimForm>(Forms.Values);
            Forms.Clear();
            foreach (DimForm form in snapshot) form.Dispose();

            if (subscribed)
            {
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                subscribed = false;
            }
        }

        private static void EnsureSubscribed()
        {
            if (subscribed) return;
            // 插拔外接屏、改分辨率之后，窗口的 Bounds 就是旧坐标，必须重新对齐
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            subscribed = true;
        }

        private static void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            if (Forms.Count == 0) return;

            try
            {
                SetBrightness(brightnessPercent);
            }
            catch (Exception ex)
            {
                // 显示器热插拔时 Screen.AllScreens 有可能短暂处于半更新状态；
                // 减光失败不该弹任何东西砸到用户脸上，记日志就够了
                Program.WriteLog("显示设置变化后重建减光遮罩失败", ex);
            }
        }

        /// <summary>一层黑色半透明、鼠标穿透、不抢焦点的减光窗口。</summary>
        private sealed class DimForm : Form
        {
            private const int WS_EX_TRANSPARENT = 0x00000020;   // 鼠标穿透：墨镜不挡操作
            private const int WS_EX_TOOLWINDOW = 0x00000080;    // 不进 Alt-Tab 列表
            private const int WS_EX_NOACTIVATE = 0x08000000;    // 点它/显示它都不抢焦点

            public DimForm(Rectangle bounds, double opacity)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                BackColor = Color.Black;
                TopMost = true;
                Bounds = bounds;

                // Opacity 必须严格小于 1：WinForms 靠它把窗口切成分层窗口（WS_EX_LAYERED），
                // 设成 1 会被取消分层，那时再想透明就只能靠 per-pixel alpha 了。
                // 所以「不遮光」走的是销毁窗口，而不是把 Opacity 调成 1。
                Opacity = opacity;

                // 纯色背景，没有子控件，不需要擦背景之外的任何东西
                DoubleBuffered = true;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            /// <summary>重新 assert 顶层（不移动、不改大小、不激活）。</summary>
            public void PushTopMost()
            {
                if (!IsHandleCreated) return;
                Native.SetWindowPos(Handle, Native.HWND_TOPMOST, 0, 0, 0, 0,
                    Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            }

            // 显示时不激活：不然 Show() 一下，用户当前正在打字窗口会失去焦点
            protected override bool ShowWithoutActivation => true;
        }

        private static class Native
        {
            public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOACTIVATE = 0x0010;

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);
        }
    }
}
