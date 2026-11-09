/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_screen.cs
 * 作者：zhenhua
 * 说明：「屏幕防拍摄」模块：隐私遮罩框、亮度调节、高频闪烁测试。亮度是两级减光——
 * 先用显卡伽马曲线压（无损、不占窗口），驱动扛不住的那一段交给 ScreenDimOverlay 的遮罩层。
 */
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinDisplay.Ui;

namespace WinDisplay
{
    public partial class frm_screen : Form
    {




        private List<WeakReference<Form>> maskForms = new List<WeakReference<Form>>(); // 管理多个遮挡窗体
        private bool isMaskLocked = false; // 默认不固定
        private const int resizeBorder = 5; // 边框区域宽度
        private bool isDragging = false;
        private bool isResizing = false;
        private Point lastMousePos;

        private bool showMaskWin = true; //默认显示遮挡窗体

        // 亮度调节：拖滑块时每动一格都会进来一次，攒一下再写驱动
        private System.Windows.Forms.Timer? brightnessDebounce;
        private bool gammaNoticeShown;  // 同一句限制提示一次会话只说一次


        Form maskForm;

        public frm_screen()
        {
            InitializeComponent();

            // 卡片上的「当前亮度 / 显卡状态 / 挡板数量」一进来就得是真的，不能等用户拖了滑块才出数
            UpdateBrightnessUi();
            UpdateMaskPill();
            this.TopMost = true;
            this.MaximizeBox = false; // 禁用最大化按钮


            // 独立运行（不嵌主窗体）时最小化仍锁挡板；嵌在主窗体里时本窗体不会自己最小化，
            // 锁挡板改走 MainForm.HideToTray → LockMasksForTray
            this.Resize += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    LockMasksForTray();
                }
            };


        }

        // 托盘已经改由主窗体 MainForm 接管：v2.0 后本窗体是嵌在 contentHost 里的子窗体，
        // 不再是顶层窗口，它自己 Show()/WindowState/Activate() 都动不了主窗口，
        // 于是「托盘图标在、点了恢复不了」。现在托盘只调用下面两个方法。

        /// <summary>托盘「恢复亮度」：屏幕被压得很暗时的兜底逃生口（界面看不清时托盘还点得到）。</summary>
        internal void RestoreBrightness()
        {
            if (IsDisposed) return;

            // 唤醒可能来自监听线程，回 UI 线程再碰控件
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(RestoreBrightness)); } catch { }
                return;
            }

            trackBar_bright.Value = trackBar_bright.Maximum;
            ApplyBrightness(100);
        }

        /// <summary>主窗体收进托盘前调用：强制锁定挡板，防止人不在时误拖误关。</summary>
        internal void LockMasksForTray()
        {
            if (IsDisposed) return;

            isMaskLocked = true;
            but_mask_lock.Text = "解锁挡板";
            UpdateMaskFormState(1);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 真正关闭时（含托盘「退出」→ Application.Exit）恢复屏幕亮度：
            // 伽马曲线是对显卡的修改，以前没有任何复位路径，调暗过后退出程序屏幕会一直暗着。
            // 退出路径上不弹框，只记日志。
            brightnessDebounce?.Dispose();
            brightnessDebounce = null;

            // 减光遮罩是 TopMost 盖在整块屏上的，不收干净就会跟着进程一直黑在那（进程崩了系统会销毁窗口，
            // 但「窗口关了但进程还活在托盘里」这条路必须自己收）
            ScreenDimOverlay.DisposeAll();

            if (!ScreenBrightnessControl.ResetBrightness(out string? brightnessError) && brightnessError != null)
            {
                Program.WriteLog("退出时恢复屏幕亮度失败", new InvalidOperationException(brightnessError));
            }

            base.OnFormClosed(e);
        }



        /// <summary>
        /// 亮度调节的统一入口：两级减光。
        /// <para>
        /// 总亮度 = 伽马亮度 × 遮罩透光率。先用显卡伽马压（画质无损、不占窗口），伽马扛不住的那段
        /// （驱动限制偏离量≤约一半，本机实测最低 50%）由 <see cref="ScreenDimOverlay"/> 接着压，
        /// 所以滑到 20% 就真的拿得到 20%，不再「拖到底就没反应」。
        /// </para>
        /// <para>
        /// 伽马彻底不可用（远程桌面、虚拟显卡）时全部交给遮罩，功能不会失效，只提示一次。
        /// </para>
        /// </summary>
        private void ApplyBrightness(int brightness)
        {
            int gamma = brightness;

            if (!ScreenBrightnessControl.TrySetBrightness(brightness, out string? gammaMessage, out gamma) || gamma <= 0)
            {
                // 这条路在本机不通：伽马按原样（100%），差额全交给遮罩
                gamma = 100;
                NoticeBrightness(gammaMessage);
            }

            // gamma >= brightness 恒成立（伽马只会往亮的一侧夹），所以算出来不会超过 100
            int overlay = (int)Math.Round(brightness * 100.0 / gamma);
            ScreenDimOverlay.SetBrightness(overlay);

            lastGammaApplied = gamma;
            UpdateBrightnessUi();
        }

        /// <summary>显卡伽马实际吃进去的那一档，剩下那段由遮罩扛。</summary>
        private int lastGammaApplied = 100;

        /// <summary>
        /// 把滑块位置、显卡伽马能吃到的下限、遮罩透光率显示到卡片上。
        /// 少了这几行，用户只知道「拖了没反应」，分不清是驱动封顶了还是程序没干活。
        /// </summary>
        private void UpdateBrightnessUi()
        {
            if (metricBrightness == null) return;

            metricBrightness.Value = trackBar_bright.Value.ToString();

            bool? supports = ScreenBrightnessControl.DeviceSupportsGamma;
            if (ScreenBrightnessControl.IsRemoteSession)
            {
                pillGamma.SetState("远程桌面 · 只用遮罩", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
            }
            else if (supports == false)
            {
                pillGamma.SetState($"显卡不支持伽马 · 只用遮罩", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
            }
            else if (ScreenBrightnessControl.FloorPercent > 0)
            {
                pillGamma.SetState($"显卡伽马最暗 {ScreenBrightnessControl.FloorPercent}%", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }
            else
            {
                pillGamma.SetState(supports == true ? "显卡伽马可用" : "伽马未实测", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }

            if (ScreenDimOverlay.IsActive)
            {
                pillOverlay.SetState($"遮罩透光 {ScreenDimOverlay.Brightness}%", UiTheme.Accent, UiTheme.AccentSoft, UiTheme.AccentText);
            }
            else
            {
                pillOverlay.SetState("未使用遮罩", UiTheme.TextTertiary, UiTheme.SurfaceMuted, UiTheme.TextSecondary);
            }
        }

        private void NoticeBrightness(string? message)
        {
            if (string.IsNullOrEmpty(message) || gammaNoticeShown) return;

            gammaNoticeShown = true;
            Program.WriteLog("伽马调光不可用，已改由遮罩承担", new InvalidOperationException(message));

            // 托盘已归主窗体，气泡也从它那里发；没有主窗体（单独跑本模块）时退回控制台
            if (MainForm.Instance != null)
            {
                MainForm.Instance.ShowBalloon("本机不支持伽马调光", message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        public void setBright(int brightness)
        {
            ApplyBrightness(brightness);
        }

        /// <summary>
        /// 档位按钮共用这一个处理器：数值写在 Tag 里，不拿按钮文字去解析（改文案不会把逻辑带歪）。
        /// 原来八个一模一样的 but_brightXX_Click 是设计器拖出来的，这里合并掉了。
        /// </summary>
        private void but_brightPreset_Click(object? sender, EventArgs e)
        {
            if (sender is not Control button || !int.TryParse(button.Tag as string, out int brightness))
            {
                return;
            }

            trackBar_bright.Value = Math.Clamp(brightness, trackBar_bright.Minimum, trackBar_bright.Maximum);
            ApplyBrightness(brightness);
        }

        private void trackBar_bright_Scroll(object sender, EventArgs e)
        {
            // 原实现：每个 Scroll 事件直接写驱动，失败就 MessageBox.Show——一拖一串错误框。
            // 现在停下 80ms 再写一次；驱动不接受的值由 ScreenBrightnessControl 夹住，不弹框。
            // （旧的 brightness > 5 判断拿掉了：那只是为了遮「低亮度其实设不进去」，现在由实测下限接管）
            if (brightnessDebounce == null)
            {
                brightnessDebounce = new System.Windows.Forms.Timer { Interval = 80 };
                brightnessDebounce.Tick += (s, args) =>
                {
                    brightnessDebounce!.Stop();
                    ApplyBrightness(trackBar_bright.Value);
                };
            }
            brightnessDebounce.Stop();   // 还在动就把上一次待执行的那下抖掉
            brightnessDebounce.Start();
        }

        private void but_maskWin_Click(object sender, EventArgs e)
        {
            Size size = new Size(200, 200);

            CreateMaskForm(size, "sub_" + maskForms.Count);

        }



        // 创建遮挡窗体
        private Form CreateMaskForm(Size size, String title)
        {
            Point point = new Point(300 + maskForms.Count * 20, 300 + maskForms.Count * 20); // 偏移避免重叠

            double opacity = 0.5;
            if (opt_apla1.Value != null)
            {
                opacity = (double)(opt_apla1.Value / 10);
            }

            var maskForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.Black,
                Opacity = opacity,
                Text = title,
                Size = size,
                Location = point,
                TopMost = true,
                ShowInTaskbar = false,// 不显示在任务栏
                KeyPreview = true,// 确保窗体能捕获键盘事件
            };

            // 添加水印。Font 是 GDI 对象，Control 不会替外部传进来的 Font 调 Dispose，
            // 所以这里持住引用，跟着遮罩窗体一起释放
            Font watermarkFont = new Font("Arial", 12);
            Label watermark = new Label
            {
                Text = txt_maskform_tip.Text + $"  {Environment.UserName}",
                ForeColor = Color.White,
                Font = watermarkFont,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            maskForm.Controls.Add(watermark);

            // 动态更新水印
            var watermarkTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            watermarkTimer.Tick += (s, e) =>
            {
                watermark.Text = txt_maskform_tip.Text + $" {Environment.UserName} at {DateTime.Now}";
            };
            watermarkTimer.Start();

            // 定时器必须跟着遮罩窗体一起释放。原来它只被闭包引用，窗体关掉（Hide + 从列表移除）
            // 之后每秒照跑，反复创建/关闭遮罩就累积成一堆活动定时器 + 永远回收不掉的窗体
            maskForm.Disposed += (s, e) =>
            {
                watermarkTimer.Dispose();
                watermarkFont.Dispose();
            };

            // 鼠标事件处理
            bool isDragging = false;
            bool isResizing = false;
            Point lastMousePos = Point.Empty;

            maskForm.MouseDown += (s, e) =>
            {
                if (!isMaskLocked && e.Button == MouseButtons.Left)
                {
                    bool inResizeArea =
                        e.X <= resizeBorder || e.X >= maskForm.Width - resizeBorder ||
                        e.Y <= resizeBorder || e.Y >= maskForm.Height - resizeBorder;

                    isResizing = inResizeArea;
                    isDragging = !inResizeArea;
                    lastMousePos = maskForm.PointToScreen(e.Location);
                }
            };

            maskForm.MouseMove += (s, e) =>
            {
                if (!isMaskLocked && e.Button == MouseButtons.Left)
                {
                    Point currentMousePos = maskForm.PointToScreen(e.Location);
                    if (isDragging)
                    {
                        int deltaX = currentMousePos.X - lastMousePos.X;
                        int deltaY = currentMousePos.Y - lastMousePos.Y;
                        maskForm.Location = new Point(maskForm.Location.X + deltaX, maskForm.Location.Y + deltaY);
                        lastMousePos = currentMousePos;
                    }
                    else if (isResizing)
                    {
                        int newWidth = maskForm.Width + (currentMousePos.X - lastMousePos.X);
                        int newHeight = maskForm.Height + (currentMousePos.Y - lastMousePos.Y);
                        maskForm.Size = new Size(Math.Max(50, newWidth), Math.Max(50, newHeight));
                        lastMousePos = currentMousePos;
                    }
                }
            };

            maskForm.MouseUp += (s, e) =>
            {
                isDragging = false;
                isResizing = false;
            };

            maskForm.KeyDown += Form1_KeyDown;


            //UpdateMaskFormState();
            maskForm.Show();

            // 遮挡窗体也是 TopMost 而且会激活，不重新顶一下它会盖到减光遮罩上面，
            // 结果就是“遮罩不遮光”
            ScreenDimOverlay.ReassertTopMost();
            // 添加到列表并初始化状态
            maskForms.Add(new WeakReference<Form>(maskForm));
            this.TopMost = true;
            UpdateMaskPill();

            return maskForm;
        }








        // 锁定/解锁按钮事件
        private void but_mask_lock_Click(object sender, EventArgs e)
        {
            isMaskLocked = !isMaskLocked;
            this.but_mask_lock.Text = isMaskLocked ? "解锁挡板" : "固定挡板";
            UpdateMaskFormState(1);
            UpdateMaskPill();
        }

        // 更新所有遮挡窗体状态
        private void UpdateMaskFormState(int type)
        {

            var formsToRemove = new List<WeakReference<Form>>(); // 临时列表
            foreach (var maskForm in maskForms)
            {


                if (maskForm.TryGetTarget(out Form form) && !form.IsDisposed)
                {

                    if (type == 1)//固定或解锁挡板
                    {
                        if (isMaskLocked)
                        {
                            SetWindowClickThrough(form.Handle); // 事件穿透
                            form.Cursor = Cursors.Default;
                        }
                        else
                        {
                            ClearWindowClickThrough(form.Handle); // 允许交互
                            form.Cursor = Cursors.SizeAll;
                        }

                    }
                    else if (type == 2)//隐藏或显示挡板
                    {
                        if (showMaskWin)
                        {
                            form.Show();
                        }
                        else
                        {
                            form.Hide();
                        }

                    }

                }
                else
                {
                    formsToRemove.Add(maskForm); // 清理无效引用
                }

            }

            // 遍历后移除
            if (formsToRemove.Count > 0)
            {
                maskForms.RemoveAll(wr => formsToRemove.Contains(wr));
            }
            formsToRemove.Clear();

        }







        // 鼠标按下事件（开始拖动或调整大小）
        private void MaskForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isMaskLocked && e.Button == MouseButtons.Left)
            {
                // 判断是否在调整大小区域（边框）
                bool inResizeArea =
                    e.X <= resizeBorder || e.X >= maskForm.Width - resizeBorder ||
                    e.Y <= resizeBorder || e.Y >= maskForm.Height - resizeBorder;

                isResizing = inResizeArea;
                isDragging = !inResizeArea;
                lastMousePos = maskForm.PointToScreen(e.Location); // 使用屏幕坐标
            }
        }

        // 鼠标移动事件（拖动或调整大小）
        private void MaskForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMaskLocked && e.Button == MouseButtons.Left)
            {
                Point currentMousePos = maskForm.PointToScreen(e.Location); // 使用屏幕坐标
                if (isDragging)
                {
                    // 拖动窗体
                    int deltaX = currentMousePos.X - lastMousePos.X;
                    int deltaY = currentMousePos.Y - lastMousePos.Y;
                    maskForm.Location = new Point(maskForm.Location.X + deltaX, maskForm.Location.Y + deltaY);
                    lastMousePos = currentMousePos;
                }
                else if (isResizing)
                {
                    // 调整大小
                    int newWidth = maskForm.Width + (currentMousePos.X - lastMousePos.X);
                    int newHeight = maskForm.Height + (currentMousePos.Y - lastMousePos.Y);
                    maskForm.Size = new Size(Math.Max(50, newWidth), Math.Max(50, newHeight));
                    lastMousePos = currentMousePos;
                }
            }
        }


        // 鼠标释放事件
        private void MaskForm_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isResizing = false;
        }





        // 设置窗体事件穿透
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);



        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        private void SetWindowClickThrough(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }



        private void ClearWindowClickThrough(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);


            // 确保只移除 WS_EX_TRANSPARENT，保留 WS_EX_LAYERED
            SetWindowLong(hWnd, GWL_EXSTYLE, (exStyle & ~WS_EX_TRANSPARENT) | WS_EX_LAYERED);
        }

        private void but_high_frequency_Click(object sender, EventArgs e)
        {
            frm_display_test form2 = new frm_display_test();
            form2.Show();
        }



        private void but_hide_Click(object sender, EventArgs e)
        {
            showMaskWindow();
        }

        private void showMaskWindow()
        {
            showMaskWin = !showMaskWin;
            this.but_hide.Text = showMaskWin ? "隐藏挡板" : "显示挡板";
            UpdateMaskFormState(2);
            UpdateMaskPill();
        }

        /// <summary>挡板数量放在卡片右上角：盖了几块、锁没锁，一眼看得见。</summary>
        private void UpdateMaskPill()
        {
            if (pillMask == null) return;

            // maskForms 存的是弱引用，用户手动关掉的那几块先清掉再计数
            maskForms.RemoveAll(w => !w.TryGetTarget(out Form? f) || f.IsDisposed);

            if (maskForms.Count == 0)
            {
                pillMask.SetState("没有挡板", UiTheme.TextTertiary, UiTheme.SurfaceMuted, UiTheme.TextSecondary);
            }
            else
            {
                pillMask.SetState($"{maskForms.Count} 块挡板 · {(isMaskLocked ? "已锁定" : "可拖动")}",
                    UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }
        }




        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Form f = sender as Form;
                if (f != null && !f.IsDisposed)
                {
                    f.Close(); // 关闭当前窗体
                }
            }
        }


        private void but_post_left_Click(object sender, EventArgs e)
        {

            if (hasMaskFormByTitle("sub_left"))
            {
                return;
            }
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            Size size = new Size(400, screenHeight);
            Form form = CreateMaskForm(size, "sub_left");

            //重新定位
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top); // 主屏幕左上角
            form.Location = point;

        }

        private bool hasMaskFormByTitle(string title)
        {
            //有窗口找到删除


            foreach (var maskForm in maskForms)
            {


                if (maskForm.TryGetTarget(out Form form) && !form.IsDisposed)
                {

                    if (!String.IsNullOrEmpty(form.Text))
                    {
                        if (title.Equals(form.Text))
                        {
                            // 原来是 Hide()：窗体既不在列表里也没被释放，谁都不会再碰到它，
                            // 但它连同每秒一次的水印定时器一直活到进程结束
                            maskForms.Remove(maskForm);
                            form.Dispose();
                            return true;
                        }
                    }

                }


            }

            return false;
        }

        private void but_post_up_Click(object sender, EventArgs e)
        {

            if (hasMaskFormByTitle("sub_up"))
            {
                return;
            }
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;

            Size size = new Size(screenWidth, 200);
            Form form = CreateMaskForm(size, "sub_up");

            //重新定位
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top); // 主屏幕左上角
            form.Location = point;

        }

        private void but_post_down_Click(object sender, EventArgs e)
        {

            if (hasMaskFormByTitle("sub_down"))
            {
                return;
            }
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;

            Size size = new Size(screenWidth, 200);
            Form form = CreateMaskForm(size, "sub_down");

            //重新定位
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Bottom - 200); // 主屏幕左上角
            form.Location = point;

        }

        private void but_post_right_Click(object sender, EventArgs e)
        {

            if (hasMaskFormByTitle("sub_right"))
            {
                return;
            }
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            Size size = new Size(400, screenHeight);
            Form form = CreateMaskForm(size, "sub_right");

            //重新定位
            Point point = new Point(Screen.PrimaryScreen.Bounds.Right - 400, Screen.PrimaryScreen.Bounds.Top); // 主屏幕左上角
            form.Location = point;
        }

    }
}
