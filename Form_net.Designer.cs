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
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            but_open_wf = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 56);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1285, 391);
            dataGridView1.TabIndex = 0;
            // 
            // but_start
            // 
            but_start.Location = new Point(929, 14);
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
            combo_devList.Location = new Point(71, 11);
            combo_devList.Name = "combo_devList";
            combo_devList.Size = new Size(853, 25);
            combo_devList.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 14);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 3;
            label1.Text = "网络接口：";
            // 
            // but_stop
            // 
            but_stop.Location = new Point(989, 14);
            but_stop.Name = "but_stop";
            but_stop.Size = new Size(54, 23);
            but_stop.TabIndex = 1;
            but_stop.Text = "停止";
            but_stop.UseVisualStyleBackColor = true;
            but_stop.Click += but_stop_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridView1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1291, 450);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(but_open_wf);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(combo_devList);
            panel1.Controls.Add(but_stop);
            panel1.Controls.Add(but_start);
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1260, 47);
            panel1.TabIndex = 5;
            // 
            // but_open_wf
            // 
            but_open_wf.Location = new Point(1049, 14);
            but_open_wf.Name = "but_open_wf";
            but_open_wf.Size = new Size(85, 22);
            but_open_wf.TabIndex = 4;
            but_open_wf.Text = "系统防火墙";
            but_open_wf.UseVisualStyleBackColor = true;
            but_open_wf.Click += but_open_wf_Click;
            // 
            // Form_net
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1291, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Form_net";
            Text = "网络安全";
            FormClosing += Form_net_FormClosing;
            Load += Form_net_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button but_start;
        private ComboBox combo_devList;
        private Label label1;
        private Button but_stop;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Button but_open_wf;
    }
}