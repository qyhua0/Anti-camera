namespace WinDisplay
{
    partial class form_clear
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
            listViewResults = new ListView();
            progressBar1 = new ProgressBar();
            lblStatus = new Label();
            btnScan = new Button();
            btnClean = new Button();
            btnSelectAll = new Button();
            btnDeselectAll = new Button();
            lblTotalSize = new Label();
            SuspendLayout();
            // 
            // listViewResults
            // 
            listViewResults.Location = new Point(12, 43);
            listViewResults.Name = "listViewResults";
            listViewResults.Size = new Size(1153, 415);
            listViewResults.TabIndex = 0;
            listViewResults.UseCompatibleStateImageBehavior = false;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(93, 11);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(962, 23);
            progressBar1.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 464);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(44, 17);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "请扫描";
            // 
            // btnScan
            // 
            btnScan.Location = new Point(12, 11);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(75, 23);
            btnScan.TabIndex = 3;
            btnScan.Text = "扫描";
            btnScan.UseVisualStyleBackColor = true;
            // 
            // btnClean
            // 
            btnClean.Location = new Point(1090, 464);
            btnClean.Name = "btnClean";
            btnClean.Size = new Size(75, 23);
            btnClean.TabIndex = 4;
            btnClean.Text = "清理";
            btnClean.UseVisualStyleBackColor = true;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Location = new Point(911, 464);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(75, 23);
            btnSelectAll.TabIndex = 5;
            btnSelectAll.Text = "选择全部";
            btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // btnDeselectAll
            // 
            btnDeselectAll.Location = new Point(1001, 464);
            btnDeselectAll.Name = "btnDeselectAll";
            btnDeselectAll.Size = new Size(75, 23);
            btnDeselectAll.TabIndex = 6;
            btnDeselectAll.Text = "取消全选";
            btnDeselectAll.UseVisualStyleBackColor = true;
            // 
            // lblTotalSize
            // 
            lblTotalSize.AutoSize = true;
            lblTotalSize.Location = new Point(1061, 14);
            lblTotalSize.Name = "lblTotalSize";
            lblTotalSize.Size = new Size(39, 17);
            lblTotalSize.TabIndex = 7;
            lblTotalSize.Text = "0 MB";
            lblTotalSize.Click += lblTotalSize_Click;
            // 
            // form_clear
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1177, 499);
            Controls.Add(lblTotalSize);
            Controls.Add(btnDeselectAll);
            Controls.Add(btnSelectAll);
            Controls.Add(btnClean);
            Controls.Add(btnScan);
            Controls.Add(lblStatus);
            Controls.Add(progressBar1);
            Controls.Add(listViewResults);
            Name = "form_clear";
            Text = "form_clear";
            Load += form_clear_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listViewResults;
        private ProgressBar progressBar1;
        private Label lblStatus;
        private Button btnScan;
        private Button btnClean;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private Label lblTotalSize;
    }
}