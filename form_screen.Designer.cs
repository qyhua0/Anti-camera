namespace WinDisplay
{
    partial class frm_screen
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_screen));
            but_bright75 = new Button();
            but_bright50 = new Button();
            but_bright70 = new Button();
            groupBox1 = new GroupBox();
            trackBar_bright = new TrackBar();
            but_bright90 = new Button();
            but_bright95 = new Button();
            but_bright85 = new Button();
            but_bright80 = new Button();
            but_bright60 = new Button();
            but_maskWin = new Button();
            but_mask_lock = new Button();
            groupBox2 = new GroupBox();
            txt_maskform_tip = new TextBox();
            label2 = new Label();
            groupBox4 = new GroupBox();
            but_post_right = new Button();
            but_post_left = new Button();
            but_post_down = new Button();
            but_post_up = new Button();
            label1 = new Label();
            opt_apla1 = new NumericUpDown();
            but_hide = new Button();
            groupBox3 = new GroupBox();
            but_high_frequency = new Button();
            _notifyIcon = new NotifyIcon(components);
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_bright).BeginInit();
            groupBox2.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opt_apla1).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // but_bright75
            // 
            but_bright75.Location = new Point(173, 22);
            but_bright75.Name = "but_bright75";
            but_bright75.Size = new Size(41, 23);
            but_bright75.TabIndex = 0;
            but_bright75.Text = "75%";
            but_bright75.UseVisualStyleBackColor = true;
            but_bright75.Click += but_bright75_Click;
            // 
            // but_bright50
            // 
            but_bright50.Location = new Point(17, 22);
            but_bright50.Name = "but_bright50";
            but_bright50.Size = new Size(41, 23);
            but_bright50.TabIndex = 0;
            but_bright50.Text = "50%";
            but_bright50.UseVisualStyleBackColor = true;
            but_bright50.Click += but_bright50_Click;
            // 
            // but_bright70
            // 
            but_bright70.Location = new Point(121, 22);
            but_bright70.Name = "but_bright70";
            but_bright70.Size = new Size(41, 23);
            but_bright70.TabIndex = 0;
            but_bright70.Text = "70%";
            but_bright70.UseVisualStyleBackColor = true;
            but_bright70.Click += but_bright70_Click;
            but_bright70.MouseCaptureChanged += but_bright70_MouseCaptureChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(trackBar_bright);
            groupBox1.Controls.Add(but_bright75);
            groupBox1.Controls.Add(but_bright90);
            groupBox1.Controls.Add(but_bright95);
            groupBox1.Controls.Add(but_bright85);
            groupBox1.Controls.Add(but_bright80);
            groupBox1.Controls.Add(but_bright70);
            groupBox1.Controls.Add(but_bright60);
            groupBox1.Controls.Add(but_bright50);
            groupBox1.Location = new Point(30, 24);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(437, 117);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "屏幕亮度控制";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // trackBar_bright
            // 
            trackBar_bright.Location = new Point(17, 66);
            trackBar_bright.Maximum = 100;
            trackBar_bright.Minimum = 1;
            trackBar_bright.Name = "trackBar_bright";
            trackBar_bright.Size = new Size(384, 45);
            trackBar_bright.TabIndex = 1;
            trackBar_bright.Value = 1;
            trackBar_bright.Scroll += trackBar_bright_Scroll;
            // 
            // but_bright90
            // 
            but_bright90.Location = new Point(329, 22);
            but_bright90.Name = "but_bright90";
            but_bright90.Size = new Size(41, 23);
            but_bright90.TabIndex = 0;
            but_bright90.Text = "90%";
            but_bright90.UseVisualStyleBackColor = true;
            but_bright90.Click += but_bright90_Click;
            // 
            // but_bright95
            // 
            but_bright95.Location = new Point(381, 22);
            but_bright95.Name = "but_bright95";
            but_bright95.Size = new Size(41, 23);
            but_bright95.TabIndex = 0;
            but_bright95.Text = "95%";
            but_bright95.UseVisualStyleBackColor = true;
            but_bright95.Click += but_bright95_Click;
            // 
            // but_bright85
            // 
            but_bright85.Location = new Point(277, 22);
            but_bright85.Name = "but_bright85";
            but_bright85.Size = new Size(41, 23);
            but_bright85.TabIndex = 0;
            but_bright85.Text = "85%";
            but_bright85.UseVisualStyleBackColor = true;
            but_bright85.Click += but_bright85_Click;
            // 
            // but_bright80
            // 
            but_bright80.Location = new Point(225, 22);
            but_bright80.Name = "but_bright80";
            but_bright80.Size = new Size(41, 23);
            but_bright80.TabIndex = 0;
            but_bright80.Text = "80%";
            but_bright80.UseVisualStyleBackColor = true;
            but_bright80.Click += but_bright80_Click;
            // 
            // but_bright60
            // 
            but_bright60.Location = new Point(69, 22);
            but_bright60.Name = "but_bright60";
            but_bright60.Size = new Size(41, 23);
            but_bright60.TabIndex = 0;
            but_bright60.Text = "60%";
            but_bright60.UseVisualStyleBackColor = true;
            but_bright60.Click += but_bright60_Click;
            // 
            // but_maskWin
            // 
            but_maskWin.Location = new Point(141, 22);
            but_maskWin.Name = "but_maskWin";
            but_maskWin.Size = new Size(65, 23);
            but_maskWin.TabIndex = 2;
            but_maskWin.Text = "添加挡板";
            but_maskWin.UseVisualStyleBackColor = true;
            but_maskWin.Click += but_maskWin_Click;
            // 
            // but_mask_lock
            // 
            but_mask_lock.Location = new Point(17, 70);
            but_mask_lock.Name = "but_mask_lock";
            but_mask_lock.Size = new Size(70, 23);
            but_mask_lock.TabIndex = 3;
            but_mask_lock.Text = "固定档板";
            but_mask_lock.UseVisualStyleBackColor = true;
            but_mask_lock.Click += but_mask_lock_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txt_maskform_tip);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(groupBox4);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(opt_apla1);
            groupBox2.Controls.Add(but_hide);
            groupBox2.Controls.Add(but_maskWin);
            groupBox2.Controls.Add(but_mask_lock);
            groupBox2.Location = new Point(30, 156);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(437, 173);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "档板保护";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // txt_maskform_tip
            // 
            txt_maskform_tip.Location = new Point(81, 109);
            txt_maskform_tip.Multiline = true;
            txt_maskform_tip.Name = "txt_maskform_tip";
            txt_maskform_tip.Size = new Size(190, 45);
            txt_maskform_tip.TabIndex = 9;
            txt_maskform_tip.Text = "保护隐私,禁止偷看";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 120);
            label2.Name = "label2";
            label2.Size = new Size(68, 17);
            label2.TabIndex = 8;
            label2.Text = "提示内容：";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(but_post_right);
            groupBox4.Controls.Add(but_post_left);
            groupBox4.Controls.Add(but_post_down);
            groupBox4.Controls.Add(but_post_up);
            groupBox4.Location = new Point(286, 22);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(138, 115);
            groupBox4.TabIndex = 7;
            groupBox4.TabStop = false;
            groupBox4.Text = "快捷操作";
            groupBox4.Enter += groupBox4_Enter;
            // 
            // but_post_right
            // 
            but_post_right.Location = new Point(84, 48);
            but_post_right.Name = "but_post_right";
            but_post_right.Size = new Size(31, 23);
            but_post_right.TabIndex = 0;
            but_post_right.Text = "右";
            but_post_right.UseVisualStyleBackColor = true;
            but_post_right.Click += but_post_right_Click;
            // 
            // but_post_left
            // 
            but_post_left.Location = new Point(10, 48);
            but_post_left.Name = "but_post_left";
            but_post_left.Size = new Size(31, 23);
            but_post_left.TabIndex = 0;
            but_post_left.Text = "左";
            but_post_left.UseVisualStyleBackColor = true;
            but_post_left.Click += but_post_left_Click;
            // 
            // but_post_down
            // 
            but_post_down.Location = new Point(47, 75);
            but_post_down.Name = "but_post_down";
            but_post_down.Size = new Size(31, 23);
            but_post_down.TabIndex = 0;
            but_post_down.Text = "下";
            but_post_down.UseVisualStyleBackColor = true;
            but_post_down.Click += but_post_down_Click;
            // 
            // but_post_up
            // 
            but_post_up.Location = new Point(47, 22);
            but_post_up.Name = "but_post_up";
            but_post_up.Size = new Size(31, 23);
            but_post_up.TabIndex = 0;
            but_post_up.Text = "上";
            but_post_up.UseVisualStyleBackColor = true;
            but_post_up.Click += but_post_up_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 28);
            label1.Name = "label1";
            label1.Size = new Size(71, 17);
            label1.TabIndex = 6;
            label1.Text = "档板透明度:";
            // 
            // opt_apla1
            // 
            opt_apla1.Location = new Point(95, 26);
            opt_apla1.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            opt_apla1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            opt_apla1.Name = "opt_apla1";
            opt_apla1.Size = new Size(40, 23);
            opt_apla1.TabIndex = 5;
            opt_apla1.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // but_hide
            // 
            but_hide.Location = new Point(141, 70);
            but_hide.Name = "but_hide";
            but_hide.Size = new Size(65, 23);
            but_hide.TabIndex = 4;
            but_hide.Text = "隐藏档板";
            but_hide.UseVisualStyleBackColor = true;
            but_hide.Click += but_hide_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(but_high_frequency);
            groupBox3.Location = new Point(24, 335);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(443, 71);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "实验功能";
            // 
            // but_high_frequency
            // 
            but_high_frequency.Location = new Point(18, 33);
            but_high_frequency.Name = "but_high_frequency";
            but_high_frequency.Size = new Size(75, 23);
            but_high_frequency.TabIndex = 0;
            but_high_frequency.Text = "高频闪烁";
            but_high_frequency.UseVisualStyleBackColor = true;
            but_high_frequency.Click += but_high_frequency_Click;
            // 
            // _notifyIcon
            // 
            _notifyIcon.Icon = (Icon)resources.GetObject("_notifyIcon.Icon");
            _notifyIcon.Text = "notifyIcon1";
            _notifyIcon.Visible = true;
            // 
            // frm_screen
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(497, 418);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frm_screen";
            Text = "屏幕保护";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_bright).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opt_apla1).EndInit();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button but_bright75;
        private Button but_bright50;
        private Button but_bright70;
        private GroupBox groupBox1;
        private TrackBar trackBar_bright;
        private Button but_maskWin;
        private Button but_mask_lock;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Button but_high_frequency;
        private NotifyIcon _notifyIcon;
        private Button but_hide;
        private Label label1;
        private NumericUpDown opt_apla1;
        private Button but_bright80;
        private GroupBox groupBox4;
        private Button but_post_left;
        private Button but_post_down;
        private Button but_post_up;
        private Button but_post_right;
        private TextBox txt_maskform_tip;
        private Label label2;
        private Button but_bright90;
        private Button but_bright95;
        private Button but_bright85;
        private Button but_bright60;
    }
}
