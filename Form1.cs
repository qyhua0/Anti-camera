using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class Form1 : Form
    {

        private List<Form> maskForms = new List<Form>(); // �������ڵ�����
        private bool isMaskLocked = false; // Ĭ�ϲ�����
        private const int resizeBorder = 5; // �߿�������
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
                // ��������Ϊ50%
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
                // ��������Ϊ50%
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

        // �����ڵ�����
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

            // ���ˮӡ
            Label watermark = new Label
            {
                Text = $"Protected by {Environment.UserName}",
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            maskForm.Controls.Add(watermark);

            // ��̬����ˮӡ
            var watermarkTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            watermarkTimer.Tick += (s, e) =>
            {
                watermark.Text = $"Protected by {Environment.UserName} at {DateTime.Now}";
            };
            watermarkTimer.Start();

            // ����¼������϶��͵�����С��
            maskForm.MouseDown += MaskForm_MouseDown;
            maskForm.MouseMove += MaskForm_MouseMove;
            maskForm.MouseUp += MaskForm_MouseUp;

            // ��ʼ������״̬������ isMaskLocked��
            UpdateMaskFormState();
            maskForm.Show();
        }

        // �����ڵ�����
        private void CreateMaskForm()
        {
            var maskForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.Black,
                Opacity = 0.5,
                Size = new Size(200, 200),
                Location = new Point(300 + maskForms.Count * 20, 300 + maskForms.Count * 20), // ƫ�Ʊ����ص�
                TopMost = true
            };

            // ���ˮӡ
            Label watermark = new Label
            {
                Text = $"Protected by {Environment.UserName}",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            maskForm.Controls.Add(watermark);

            // ��̬����ˮӡ
            var watermarkTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            watermarkTimer.Tick += (s, e) =>
            {
                watermark.Text = $"Protected by {Environment.UserName} at {DateTime.Now}";
            };
            watermarkTimer.Start();

            // ����¼�����
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

            // ��ӵ��б���ʼ��״̬
            maskForms.Add(maskForm);
            UpdateMaskFormState();
            maskForm.Show();
        }








        // ����/������ť�¼�
        private void but_mask_lock_Click(object sender, EventArgs e)
        {
            isMaskLocked = !isMaskLocked;
            this.but_mask_lock.Text = isMaskLocked ? "����" : "����";
            UpdateMaskFormState();
        }

        // ���������ڵ�����״̬
        private void UpdateMaskFormState()
        {
            foreach (var maskForm in maskForms)
            {
                if (isMaskLocked)
                {
                    SetWindowClickThrough(maskForm.Handle); // �¼���͸
                    maskForm.Cursor = Cursors.Default;
                }
                else
                {
                    ClearWindowClickThrough(maskForm.Handle); // ������
                    maskForm.Cursor = Cursors.SizeAll;
                }
            }
        }















        // ��갴���¼�����ʼ�϶��������С��
        private void MaskForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isMaskLocked && e.Button == MouseButtons.Left)
            {
                // �ж��Ƿ��ڵ�����С���򣨱߿�
                bool inResizeArea =
                    e.X <= resizeBorder || e.X >= maskForm.Width - resizeBorder ||
                    e.Y <= resizeBorder || e.Y >= maskForm.Height - resizeBorder;

                isResizing = inResizeArea;
                isDragging = !inResizeArea;
                lastMousePos = maskForm.PointToScreen(e.Location); // ʹ����Ļ����
            }
        }

        // ����ƶ��¼����϶��������С��
        private void MaskForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMaskLocked && e.Button == MouseButtons.Left)
            {
                Point currentMousePos = maskForm.PointToScreen(e.Location); // ʹ����Ļ����
                if (isDragging)
                {
                    // �϶�����
                    int deltaX = currentMousePos.X - lastMousePos.X;
                    int deltaY = currentMousePos.Y - lastMousePos.Y;
                    maskForm.Location = new Point(maskForm.Location.X + deltaX, maskForm.Location.Y + deltaY);
                    lastMousePos = currentMousePos;
                }
                else if (isResizing)
                {
                    // ������С
                    int newWidth = maskForm.Width + (currentMousePos.X - lastMousePos.X);
                    int newHeight = maskForm.Height + (currentMousePos.Y - lastMousePos.Y);
                    maskForm.Size = new Size(Math.Max(50, newWidth), Math.Max(50, newHeight));
                    lastMousePos = currentMousePos;
                }
            }
        }


        // ����ͷ��¼�
        private void MaskForm_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isResizing = false;
        }





        // ���ô����¼���͸
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
        //    this.but_mask_lock.Text = isMaskLocked ? "����" : "����";
        //    UpdateMaskFormState();
        //}

        // �����ڵ�����״̬
        //private void UpdateMaskFormState()
        //{
        //    if (isMaskLocked)
        //    {
        //        SetWindowClickThrough(maskForm.Handle); // �¼���͸
        //        maskForm.Cursor = Cursors.Default;
        //    }
        //    else
        //    {
        //        Console.WriteLine("����");
        //        ClearWindowClickThrough(maskForm.Handle); // ������
        //        maskForm.Cursor = Cursors.SizeAll; // ��ʾ�϶����
        //    }
        //}

        private void ClearWindowClickThrough(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
 

            // ȷ��ֻ�Ƴ� WS_EX_TRANSPARENT������ WS_EX_LAYERED
            SetWindowLong(hWnd, GWL_EXSTYLE, (exStyle & ~WS_EX_TRANSPARENT) | WS_EX_LAYERED);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
