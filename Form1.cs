using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class Form1 : Form
    {

        private ContextMenuStrip trayMenu;


        private List<WeakReference<Form>> maskForms = new List<WeakReference<Form>>(); // 管理多个遮挡窗体
        private bool isMaskLocked = false; // 默认不固定
        private const int resizeBorder = 5; // 边框区域宽度
        private bool isDragging = false;
        private bool isResizing = false;
        private Point lastMousePos;

        private bool showMaskWin = true; //默认显示遮挡窗体


        Form maskForm;

        public Form1()
        {
            InitializeComponent();
            //setBright(50);
            InitializeSystemTray();
            this.TopMost = true;

        }

        private void InitializeSystemTray()
        {
            // 创建托盘菜单
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("显示窗口", null, OnShow);
            trayMenu.Items.Add("退出", null, OnExit);

            _notifyIcon.ContextMenuStrip = trayMenu;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "防摄像头app";

            // 托盘图标双击事件
            _notifyIcon.DoubleClick += OnShow;
        }


        // 显示窗口
        private void OnShow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        // 退出程序
        private void OnExit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false; // 先隐藏托盘图标
            Application.Exit();
        }

        // 窗体关闭时最小化到托盘
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                //最小化时强制锁定，防止误操作
                isMaskLocked =true;
                this.but_mask_lock.Text = "解锁档板" ;
                UpdateMaskFormState(1);

                this.Hide(); 

                _notifyIcon.ShowBalloonTip(1000, "提示", "程序已最小化到系统托盘", ToolTipIcon.Info);
            }
            base.OnFormClosing(e);
        }



        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 设置亮度为50%
                ScreenBrightnessControl.SetBrightness(50);
                Console.WriteLine("Brightness set to 50%");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void but_bright30_Click(object sender, EventArgs e)
        {
            setBright(30);
        }

        private void but_bright50_Click(object sender, EventArgs e)
        {
            setBright(50);
        }

        public void setBright(int brightness)
        {
            try
            {
                // 设置亮度为50%
                ScreenBrightnessControl.SetBrightness(brightness);
                Console.WriteLine("Brightness set to " + brightness + "%");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void but_bright70_MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void but_bright70_Click(object sender, EventArgs e)
        {
            setBright(70);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void trackBar_bright_Scroll(object sender, EventArgs e)
        {
            try
            {
                int brightness = trackBar_bright.Value;
                if (brightness > 5 && brightness <= 100)
                {
                    ScreenBrightnessControl.SetBrightnessAPI(brightness);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
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

            // 添加水印
            Label watermark = new Label
            {
                Text = txt_maskform_tip.Text+$"  {Environment.UserName}",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            maskForm.Controls.Add(watermark);

            // 动态更新水印
            var watermarkTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            watermarkTimer.Tick += (s, e) =>
            {
                watermark.Text = txt_maskform_tip.Text+$" {Environment.UserName} at {DateTime.Now}";
            };
            watermarkTimer.Start();

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
            // 添加到列表并初始化状态
            maskForms.Add(new WeakReference<Form>(maskForm));
            this.TopMost = true;

            return maskForm;
        }








        // 锁定/解锁按钮事件
        private void but_mask_lock_Click(object sender, EventArgs e)
        {
            isMaskLocked = !isMaskLocked;
            this.but_mask_lock.Text = isMaskLocked ? "解锁档板" : "固定档板";
            UpdateMaskFormState(1);
        }

        // 更新所有遮挡窗体状态
        private void UpdateMaskFormState(int type)
        {

            var formsToRemove = new List<WeakReference<Form>>(); // 临时列表
            foreach (var maskForm in maskForms)
            {


                if (maskForm.TryGetTarget(out Form form) && !form.IsDisposed)
                {

                    if (type == 1)//固定或解锁档板
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
                    else if (type == 2)//隐藏或显示档板
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

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void but_high_frequency_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }


        private void ExitApplication()
        {
            _notifyIcon.Visible = false; // 隐藏托盘图
            Application.Exit(); // 完全退出程序
        }

        private void but_hide_Click(object sender, EventArgs e)
        {
            showMaskWindow();
        }

        private void showMaskWindow()
        {
            showMaskWin = !showMaskWin;
            this.but_hide.Text = showMaskWin ? "隐藏档板" : "显示档板";
            UpdateMaskFormState(2);
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

        private void but_bright90_Click(object sender, EventArgs e)
        {
            setBright(90);
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

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
                            form.Hide();
                            maskForms.Remove(maskForm);
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

        private void button2_Click(object sender, EventArgs e)
        {
            
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
            Point point = new Point(Screen.PrimaryScreen.Bounds.Right-400, Screen.PrimaryScreen.Bounds.Top); // 主屏幕左上角
            form.Location = point;
        }
    }
}
