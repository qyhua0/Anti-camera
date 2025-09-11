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
            groupBox3 = new GroupBox();
            but_procexp = new Button();
            but_autoruns = new Button();
            groupBox2 = new GroupBox();
            but_computer = new Button();
            but_res_mon = new Button();
            but_user = new Button();
            but_task = new Button();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // but_evn
            // 
            but_evn.Location = new Point(41, 46);
            but_evn.Name = "but_evn";
            but_evn.Size = new Size(75, 23);
            but_evn.TabIndex = 0;
            but_evn.Text = "事件管理器";
            but_evn.UseVisualStyleBackColor = true;
            but_evn.Click += but_evn_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(groupBox3);
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(765, 409);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "安全面板";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(but_procexp);
            groupBox3.Controls.Add(but_autoruns);
            groupBox3.Location = new Point(17, 194);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(725, 189);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "第三方安全工具";
            // 
            // but_procexp
            // 
            but_procexp.Location = new Point(141, 47);
            but_procexp.Name = "but_procexp";
            but_procexp.Size = new Size(75, 23);
            but_procexp.TabIndex = 0;
            but_procexp.Text = "procexp";
            but_procexp.UseVisualStyleBackColor = true;
            but_procexp.Click += but_procexp_Click;
            // 
            // but_autoruns
            // 
            but_autoruns.Location = new Point(41, 47);
            but_autoruns.Name = "but_autoruns";
            but_autoruns.Size = new Size(75, 23);
            but_autoruns.TabIndex = 0;
            but_autoruns.Text = "Autoruns";
            but_autoruns.UseVisualStyleBackColor = true;
            but_autoruns.Click += but_autoruns_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(but_computer);
            groupBox2.Controls.Add(but_evn);
            groupBox2.Controls.Add(but_res_mon);
            groupBox2.Controls.Add(but_user);
            groupBox2.Controls.Add(but_task);
            groupBox2.Location = new Point(17, 48);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(725, 123);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "win10系统工具";
            // 
            // but_computer
            // 
            but_computer.Location = new Point(450, 46);
            but_computer.Name = "but_computer";
            but_computer.Size = new Size(75, 23);
            but_computer.TabIndex = 0;
            but_computer.Text = "系统管理";
            but_computer.UseVisualStyleBackColor = true;
            but_computer.Click += but_computer_Click;
            // 
            // but_res_mon
            // 
            but_res_mon.Location = new Point(141, 46);
            but_res_mon.Name = "but_res_mon";
            but_res_mon.Size = new Size(75, 23);
            but_res_mon.TabIndex = 0;
            but_res_mon.Text = "资源监控";
            but_res_mon.UseVisualStyleBackColor = true;
            but_res_mon.Click += but_res_mon_Click;
            // 
            // but_user
            // 
            but_user.Location = new Point(342, 46);
            but_user.Name = "but_user";
            but_user.Size = new Size(75, 23);
            but_user.TabIndex = 0;
            but_user.Text = "用户管理器";
            but_user.UseVisualStyleBackColor = true;
            but_user.Click += but_user_Click;
            // 
            // but_task
            // 
            but_task.Location = new Point(237, 46);
            but_task.Name = "but_task";
            but_task.Size = new Size(75, 23);
            but_task.TabIndex = 0;
            but_task.Text = "任务管理器";
            but_task.UseVisualStyleBackColor = true;
            but_task.Click += but_task_Click;
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
            groupBox3.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button but_evn;
        private GroupBox groupBox1;
        private Button but_computer;
        private Button but_user;
        private Button but_task;
        private Button but_res_mon;
        private GroupBox groupBox3;
        private Button but_autoruns;
        private GroupBox groupBox2;
        private Button but_procexp;
    }
}