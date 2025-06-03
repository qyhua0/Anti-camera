using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class Form1 : Form
    {

        private List<Form> maskForms = new List<Form>(); // 管理多个遮挡窗体
        private bool isMaskLocked = false; // 默认不锁定
        private const int resizeBorder = 5; // 边框区域宽度
        private bool isDragging = false;
        private bool isResizing = false;
        private Point lastMousePos;


        Form maskForm;

        public Form1()
        {
            InitializeComponent();
            //setBright(50);
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
            CreateMaskForm();
        }

        // 创建遮挡窗体
        private void CreateMaskFormOne()
        {
            maskForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.Black,
                Opacity = 0.5,
                Size = new Size(200, 200),
                Location = new Point(300, 300),
                TopMost = true
            };

            // 添加水印
            Label watermark = new Label
            {
                Text = $"Protected by {Environment.UserName}",
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            maskForm.Controls.Add(watermark);

            // 动态更新水印
            var watermarkTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            watermarkTimer.Tick += (s, e) =>
            {
                watermark.Text = $"Protected by {Environment.UserName} at {DateTime.Now}";
            };
            watermarkTimer.Start();

            // 鼠标事件处理（拖动和调整大小）
            maskForm.MouseDown += MaskForm_MouseDown;
            maskForm.MouseMove += MaskForm_MouseMove;
            maskForm.MouseUp += MaskForm_MouseUp;

            // 初始化窗体状态（根据 isMaskLocked）
            UpdateMaskFormState();
            maskForm.Show();
        }

        // 创建遮挡窗体
        private void CreateMaskForm()
        {
            var maskForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.Black,
                Opacity = 0.5,
                Size = new Size(200, 200),
                Location = new Point(300 + maskForms.Count * 20, 300 + maskForms.Count * 20), // 偏移避免重叠
                TopMost = true
            };

            // 添加水印
            Label watermark = new Label
            {
                Text = $"Protected by {Environment.UserName}",
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
                watermark.Text = $"Protected by {Environment.UserName} at {DateTime.Now}";
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

            // 添加到列表并初始化状态
            maskForms.Add(maskForm);
            UpdateMaskFormState();
            maskForm.Show();
        }








        // 锁定/解锁按钮事件
        private void but_mask_lock_Click(object sender, EventArgs e)
        {
            isMaskLocked = !isMaskLocked;
            this.but_mask_lock.Text = isMaskLocked ? "解锁" : "锁定";
            UpdateMaskFormState();
        }

        // 更新所有遮挡窗体状态
        private void UpdateMaskFormState()
        {
            foreach (var maskForm in maskForms)
            {
                if (isMaskLocked)
                {
                    SetWindowClickThrough(maskForm.Handle); // 事件穿透
                    maskForm.Cursor = Cursors.Default;
                }
                else
                {
                    ClearWindowClickThrough(maskForm.Handle); // 允许交互
                    maskForm.Cursor = Cursors.SizeAll;
                }
            }
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

        //private void but_mask_lock_Click(object sender, EventArgs e)
        //{
        //    isMaskLocked = !isMaskLocked;
        //    this.but_mask_lock.Text = isMaskLocked ? "解锁" : "锁定";
        //    UpdateMaskFormState();
        //}

        // 更新遮挡窗体状态
        //private void UpdateMaskFormState()
        //{
        //    if (isMaskLocked)
        //    {
        //        SetWindowClickThrough(maskForm.Handle); // 事件穿透
        //        maskForm.Cursor = Cursors.Default;
        //    }
        //    else
        //    {
        //        Console.WriteLine("交互");
        //        ClearWindowClickThrough(maskForm.Handle); // 允许交互
        //        maskForm.Cursor = Cursors.SizeAll; // 显示拖动光标
        //    }
        //}

        private void ClearWindowClickThrough(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
 

            // 确保只移除 WS_EX_TRANSPARENT，保留 WS_EX_LAYERED
            SetWindowLong(hWnd, GWL_EXSTYLE, (exStyle & ~WS_EX_TRANSPARENT) | WS_EX_LAYERED);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
