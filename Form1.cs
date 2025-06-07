using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class Form1 : Form
    {

        private ContextMenuStrip trayMenu;


        private List<WeakReference<Form>> maskForms = new List<WeakReference<Form>>(); // �������ڵ�����
        private bool isMaskLocked = false; // Ĭ�ϲ��̶�
        private const int resizeBorder = 5; // �߿�������
        private bool isDragging = false;
        private bool isResizing = false;
        private Point lastMousePos;

        private bool showMaskWin = true; //Ĭ����ʾ�ڵ�����


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
            // �������̲˵�
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("��ʾ����", null, OnShow);
            trayMenu.Items.Add("�˳�", null, OnExit);

            _notifyIcon.ContextMenuStrip = trayMenu;
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "������ͷapp";

            // ����ͼ��˫���¼�
            _notifyIcon.DoubleClick += OnShow;
        }


        // ��ʾ����
        private void OnShow(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        // �˳�����
        private void OnExit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false; // ����������ͼ��
            Application.Exit();
        }

        // ����ر�ʱ��С��������
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                //��С��ʱǿ����������ֹ�����
                isMaskLocked =true;
                this.but_mask_lock.Text = "��������" ;
                UpdateMaskFormState(1);

                this.Hide(); 

                _notifyIcon.ShowBalloonTip(1000, "��ʾ", "��������С����ϵͳ����", ToolTipIcon.Info);
            }
            base.OnFormClosing(e);
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
            Size size = new Size(200, 200);

            CreateMaskForm(size, "sub_" + maskForms.Count);

        }



        // �����ڵ�����
        private Form CreateMaskForm(Size size, String title)
        {
            Point point = new Point(300 + maskForms.Count * 20, 300 + maskForms.Count * 20); // ƫ�Ʊ����ص�

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
                ShowInTaskbar = false,// ����ʾ��������
                KeyPreview = true,// ȷ�������ܲ�������¼�
            };

            // ���ˮӡ
            Label watermark = new Label
            {
                Text = txt_maskform_tip.Text+$"  {Environment.UserName}",
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
                watermark.Text = txt_maskform_tip.Text+$" {Environment.UserName} at {DateTime.Now}";
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

            maskForm.KeyDown += Form1_KeyDown;


            //UpdateMaskFormState();
            maskForm.Show();
            // ��ӵ��б���ʼ��״̬
            maskForms.Add(new WeakReference<Form>(maskForm));
            this.TopMost = true;

            return maskForm;
        }








        // ����/������ť�¼�
        private void but_mask_lock_Click(object sender, EventArgs e)
        {
            isMaskLocked = !isMaskLocked;
            this.but_mask_lock.Text = isMaskLocked ? "��������" : "�̶�����";
            UpdateMaskFormState(1);
        }

        // ���������ڵ�����״̬
        private void UpdateMaskFormState(int type)
        {

            var formsToRemove = new List<WeakReference<Form>>(); // ��ʱ�б�
            foreach (var maskForm in maskForms)
            {


                if (maskForm.TryGetTarget(out Form form) && !form.IsDisposed)
                {

                    if (type == 1)//�̶����������
                    {
                        if (isMaskLocked)
                        {
                            SetWindowClickThrough(form.Handle); // �¼���͸
                            form.Cursor = Cursors.Default;
                        }
                        else
                        {
                            ClearWindowClickThrough(form.Handle); // ������
                            form.Cursor = Cursors.SizeAll;
                        }

                    }
                    else if (type == 2)//���ػ���ʾ����
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
                    formsToRemove.Add(maskForm); // ������Ч����
                }

            }

            // �������Ƴ�
            if (formsToRemove.Count > 0)
            {
                maskForms.RemoveAll(wr => formsToRemove.Contains(wr));
            }
            formsToRemove.Clear();

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



        private void ClearWindowClickThrough(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);


            // ȷ��ֻ�Ƴ� WS_EX_TRANSPARENT������ WS_EX_LAYERED
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
            _notifyIcon.Visible = false; // ��������ͼ
            Application.Exit(); // ��ȫ�˳�����
        }

        private void but_hide_Click(object sender, EventArgs e)
        {
            showMaskWindow();
        }

        private void showMaskWindow()
        {
            showMaskWin = !showMaskWin;
            this.but_hide.Text = showMaskWin ? "���ص���" : "��ʾ����";
            UpdateMaskFormState(2);
        }




        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Form f = sender as Form;
                if (f != null && !f.IsDisposed)
                {
                    f.Close(); // �رյ�ǰ����
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

            //���¶�λ
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top); // ����Ļ���Ͻ�
            form.Location = point;

        }

        private bool hasMaskFormByTitle(string title)
        {
            //�д����ҵ�ɾ��
     

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

            //���¶�λ
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top); // ����Ļ���Ͻ�
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

            //���¶�λ
            Point point = new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Bottom - 200); // ����Ļ���Ͻ�
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

            //���¶�λ
            Point point = new Point(Screen.PrimaryScreen.Bounds.Right-400, Screen.PrimaryScreen.Bounds.Top); // ����Ļ���Ͻ�
            form.Location = point;
        }
    }
}
