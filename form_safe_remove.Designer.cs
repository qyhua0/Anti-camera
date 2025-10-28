namespace WinDisplay
{
    partial class form_safe_remove
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            but_exec = new Button();
            label1 = new Label();
            txtPath = new TextBox();
            but_selFile = new Button();
            cb_fillZero = new CheckBox();
            label2 = new Label();
            num_count = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)num_count).BeginInit();
            SuspendLayout();
            // 
            // but_exec
            // 
            but_exec.Location = new Point(352, 84);
            but_exec.Name = "but_exec";
            but_exec.Size = new Size(140, 77);
            but_exec.TabIndex = 0;
            but_exec.Text = "粉碎文件";
            but_exec.UseVisualStyleBackColor = true;
            but_exec.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 29);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 1;
            label1.Text = "选择目录：";
            // 
            // txtPath
            // 
            txtPath.Location = new Point(94, 29);
            txtPath.Name = "txtPath";
            txtPath.Size = new Size(420, 23);
            txtPath.TabIndex = 2;
            // 
            // but_selFile
            // 
            but_selFile.Location = new Point(496, 29);
            but_selFile.Name = "but_selFile";
            but_selFile.Size = new Size(18, 23);
            but_selFile.TabIndex = 0;
            but_selFile.Text = "…";
            but_selFile.UseVisualStyleBackColor = true;
            but_selFile.Click += but_selFile_Click;
            // 
            // cb_fillZero
            // 
            cb_fillZero.AutoSize = true;
            cb_fillZero.Checked = true;
            cb_fillZero.CheckState = CheckState.Checked;
            cb_fillZero.Location = new Point(94, 130);
            cb_fillZero.Name = "cb_fillZero";
            cb_fillZero.Size = new Size(82, 21);
            cb_fillZero.TabIndex = 3;
            cb_fillZero.Text = "是否填充0";
            cb_fillZero.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(94, 87);
            label2.Name = "label2";
            label2.Size = new Size(68, 17);
            label2.TabIndex = 4;
            label2.Text = "覆盖次数：";
            // 
            // num_count
            // 
            num_count.Location = new Point(168, 84);
            num_count.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            num_count.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            num_count.Name = "num_count";
            num_count.Size = new Size(77, 23);
            num_count.TabIndex = 5;
            num_count.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // form_safe_remove
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 278);
            Controls.Add(num_count);
            Controls.Add(label2);
            Controls.Add(cb_fillZero);
            Controls.Add(label1);
            Controls.Add(but_selFile);
            Controls.Add(but_exec);
            Controls.Add(txtPath);
            Name = "form_safe_remove";
            Text = "文件清除";
            ((System.ComponentModel.ISupportInitialize)num_count).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button but_exec;
        private Label label1;
        private TextBox txtPath;
        private Button but_selFile;
        private CheckBox cb_fillZero;
        private Label label2;
        private NumericUpDown num_count;
    }
}