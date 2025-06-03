namespace WinDisplay
{
    partial class Form1
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
            but_bright30 = new Button();
            but_bright50 = new Button();
            but_bright70 = new Button();
            groupBox1 = new GroupBox();
            trackBar_bright = new TrackBar();
            but_maskWin = new Button();
            but_mask_lock = new Button();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            but_high_frequency = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_bright).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // but_bright30
            // 
            but_bright30.Location = new Point(17, 22);
            but_bright30.Name = "but_bright30";
            but_bright30.Size = new Size(75, 23);
            but_bright30.TabIndex = 0;
            but_bright30.Text = "30%";
            but_bright30.UseVisualStyleBackColor = true;
            but_bright30.Click += but_bright30_Click;
            // 
            // but_bright50
            // 
            but_bright50.Location = new Point(113, 22);
            but_bright50.Name = "but_bright50";
            but_bright50.Size = new Size(75, 23);
            but_bright50.TabIndex = 0;
            but_bright50.Text = "50%";
            but_bright50.UseVisualStyleBackColor = true;
            but_bright50.Click += but_bright50_Click;
            // 
            // but_bright70
            // 
            but_bright70.Location = new Point(214, 22);
            but_bright70.Name = "but_bright70";
            but_bright70.Size = new Size(75, 23);
            but_bright70.TabIndex = 0;
            but_bright70.Text = "70%";
            but_bright70.UseVisualStyleBackColor = true;
            but_bright70.Click += but_bright70_Click;
            but_bright70.MouseCaptureChanged += but_bright70_MouseCaptureChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(trackBar_bright);
            groupBox1.Controls.Add(but_bright30);
            groupBox1.Controls.Add(but_bright70);
            groupBox1.Controls.Add(but_bright50);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(367, 117);
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
            trackBar_bright.Size = new Size(330, 45);
            trackBar_bright.TabIndex = 1;
            trackBar_bright.Value = 1;
            trackBar_bright.Scroll += trackBar_bright_Scroll;
            // 
            // but_maskWin
            // 
            but_maskWin.Location = new Point(17, 28);
            but_maskWin.Name = "but_maskWin";
            but_maskWin.Size = new Size(75, 23);
            but_maskWin.TabIndex = 2;
            but_maskWin.Text = "挡板";
            but_maskWin.UseVisualStyleBackColor = true;
            but_maskWin.Click += but_maskWin_Click;
            // 
            // but_mask_lock
            // 
            but_mask_lock.Location = new Point(130, 28);
            but_mask_lock.Name = "but_mask_lock";
            but_mask_lock.Size = new Size(75, 23);
            but_mask_lock.TabIndex = 3;
            but_mask_lock.Text = "锁定";
            but_mask_lock.UseVisualStyleBackColor = true;
            but_mask_lock.Click += but_mask_lock_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(but_maskWin);
            groupBox2.Controls.Add(but_mask_lock);
            groupBox2.Location = new Point(12, 135);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(367, 72);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "档板保护";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(but_high_frequency);
            groupBox3.Location = new Point(12, 216);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(367, 71);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "其它";
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(397, 301);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form1";
            Text = "x-保护";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar_bright).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button but_bright30;
        private Button but_bright50;
        private Button but_bright70;
        private GroupBox groupBox1;
        private TrackBar trackBar_bright;
        private Button but_maskWin;
        private Button but_mask_lock;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Button but_high_frequency;
    }
}
