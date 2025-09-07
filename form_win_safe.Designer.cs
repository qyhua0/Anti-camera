namespace WinDisplay
{
    partial class form_win_safe
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
            but_evn = new Button();
            groupBox1 = new GroupBox();
            but_computer = new Button();
            but_user = new Button();
            but_task = new Button();
            but_res_mon = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // but_evn
            // 
            but_evn.Location = new Point(17, 31);
            but_evn.Name = "but_evn";
            but_evn.Size = new Size(75, 23);
            but_evn.TabIndex = 0;
            but_evn.Text = "事件管理器";
            but_evn.UseVisualStyleBackColor = true;
            but_evn.Click += but_evn_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(but_computer);
            groupBox1.Controls.Add(but_user);
            groupBox1.Controls.Add(but_task);
            groupBox1.Controls.Add(but_res_mon);
            groupBox1.Controls.Add(but_evn);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(765, 409);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "常用安全功能";
            // 
            // but_computer
            // 
            but_computer.Location = new Point(426, 31);
            but_computer.Name = "but_computer";
            but_computer.Size = new Size(75, 23);
            but_computer.TabIndex = 0;
            but_computer.Text = "系统管理";
            but_computer.UseVisualStyleBackColor = true;
            but_computer.Click += but_computer_Click;
            // 
            // but_user
            // 
            but_user.Location = new Point(318, 31);
            but_user.Name = "but_user";
            but_user.Size = new Size(75, 23);
            but_user.TabIndex = 0;
            but_user.Text = "用户管理器";
            but_user.UseVisualStyleBackColor = true;
            but_user.Click += but_user_Click;
            // 
            // but_task
            // 
            but_task.Location = new Point(213, 31);
            but_task.Name = "but_task";
            but_task.Size = new Size(75, 23);
            but_task.TabIndex = 0;
            but_task.Text = "任务管理器";
            but_task.UseVisualStyleBackColor = true;
            but_task.Click += but_task_Click;
            // 
            // but_res_mon
            // 
            but_res_mon.Location = new Point(117, 31);
            but_res_mon.Name = "but_res_mon";
            but_res_mon.Size = new Size(75, 23);
            but_res_mon.TabIndex = 0;
            but_res_mon.Text = "资源监控";
            but_res_mon.UseVisualStyleBackColor = true;
            but_res_mon.Click += but_res_mon_Click;
            // 
            // form_win_safe
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(788, 434);
            Controls.Add(groupBox1);
            Name = "form_win_safe";
            Text = "windows 安全功能面板";
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button but_evn;
        private GroupBox groupBox1;
        private Button but_computer;
        private Button but_user;
        private Button but_task;
        private Button but_res_mon;
    }
}