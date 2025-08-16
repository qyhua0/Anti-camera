namespace WinDisplay
{
    partial class Form_net
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
            dataGridView1 = new DataGridView();
            but_start = new Button();
            combo_devList = new ComboBox();
            label1 = new Label();
            but_stop = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 38);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1134, 400);
            dataGridView1.TabIndex = 0;
            // 
            // but_start
            // 
            but_start.Location = new Point(1003, 7);
            but_start.Name = "but_start";
            but_start.Size = new Size(54, 23);
            but_start.TabIndex = 1;
            but_start.Text = "启动";
            but_start.UseVisualStyleBackColor = true;
            but_start.Click += but_start_Click;
            // 
            // combo_devList
            // 
            combo_devList.FormattingEnabled = true;
            combo_devList.Location = new Point(86, 7);
            combo_devList.Name = "combo_devList";
            combo_devList.Size = new Size(911, 25);
            combo_devList.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 3;
            label1.Text = "网络接口：";
            // 
            // but_stop
            // 
            but_stop.Location = new Point(1063, 7);
            but_stop.Name = "but_stop";
            but_stop.Size = new Size(54, 23);
            but_stop.TabIndex = 1;
            but_stop.Text = "停止";
            but_stop.UseVisualStyleBackColor = true;
            // 
            // Form_net
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1158, 450);
            Controls.Add(label1);
            Controls.Add(combo_devList);
            Controls.Add(but_stop);
            Controls.Add(but_start);
            Controls.Add(dataGridView1);
            Name = "Form_net";
            Text = "网络安全";
            FormClosing += Form_net_FormClosing;
            Load += Form_net_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button but_start;
        private ComboBox combo_devList;
        private Label label1;
        private Button but_stop;
    }
}