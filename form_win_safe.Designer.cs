/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_win_safe.Designer.cs
 * 作者：zhenhua
 * 说明：「安全功能面板」模块的设计器生成代码（分组卡片、按钮流式布局、权限状态胶囊）。
 */
using WinDisplay.Ui;

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
            toolTips?.Dispose();   // 手写设计器里没有 components 容器，气泡自己回收
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            root = new TableLayoutPanel();
            header = new PageHeader();
            body = new TableLayoutPanel();
            cardSystem = new CardPanel();
            systemFlow = new FlowLayoutPanel();
            but_evn = new FlatButton();
            but_res_mon = new FlatButton();
            but_task = new FlatButton();
            but_user = new FlatButton();
            but_computer = new FlatButton();
            cardThird = new CardPanel();
            thirdBody = new TableLayoutPanel();
            thirdFlow = new FlowLayoutPanel();
            but_autoruns = new FlatButton();
            but_procexp = new FlatButton();
            lblThirdHint = new Label();
            statusRow = new TableLayoutPanel();
            pillAdmin = new StatusPill();
            lblStatus = new Label();

            root.SuspendLayout();
            body.SuspendLayout();
            cardSystem.SuspendLayout();
            systemFlow.SuspendLayout();
            cardThird.SuspendLayout();
            thirdBody.SuspendLayout();
            thirdFlow.SuspendLayout();
            statusRow.SuspendLayout();
            SuspendLayout();

            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(body, 0, 1);
            root.Dock = DockStyle.Fill;
            root.Name = "root";
            root.Padding = new Padding(UiTheme.PagePadding, UiTheme.GapM, UiTheme.PagePadding, UiTheme.GapM);
            root.RowCount = 2;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            //
            // header
            //
            header.BackColor = UiTheme.WindowBackground;
            header.Description = "一键打开系统自带的管理与排查工具；已经在跑的会直接切过去，不重复开。";
            header.Dock = DockStyle.Fill;
            header.Name = "header";
            header.Title = "安全功能面板";

            //
            // body：两张卡片并排，最后一行留白
            //
            body.ColumnCount = 2;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 480F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Controls.Add(cardSystem, 0, 0);
            body.Controls.Add(cardThird, 1, 0);
            body.Controls.Add(statusRow, 0, 1);
            body.SetColumnSpan(statusRow, 2);
            body.Dock = DockStyle.Fill;
            body.Name = "body";
            body.RowCount = 2;
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            //
            // cardSystem：系统自带工具
            //
            cardSystem.Controls.Add(systemFlow);
            cardSystem.Description = "都是 Windows 自带的，不需要额外安装";
            cardSystem.Dock = DockStyle.Fill;
            cardSystem.Name = "cardSystem";
            cardSystem.Title = "系统工具";

            systemFlow.Dock = DockStyle.Top;
            systemFlow.AutoSize = true;
            systemFlow.Name = "systemFlow";
            systemFlow.Padding = new Padding(UiTheme.GapL, UiTheme.GapM, UiTheme.GapL, UiTheme.GapM);

            Tool(but_evn, "but_evn", "事件查看器", "看系统安全、应用程序日志，查谁在什么时候动过机器", but_evn_Click);
            Tool(but_res_mon, "but_res_mon", "资源监视器", "看 CPU / 内存 / 磁盘 / 网络被哪个进程占用", but_res_mon_Click);
            Tool(but_task, "but_task", "任务管理器", "结束进程、看启动项", but_task_Click);
            Tool(but_user, "but_user", "本地用户和组", "管理账户与组（家庭版没有，会自动转去设置页）", but_user_Click);
            Tool(but_computer, "but_computer", "计算机管理", "计划任务、服务、磁盘、设备管理器", but_computer_Click);

            systemFlow.Controls.Add(but_evn);
            systemFlow.Controls.Add(but_res_mon);
            systemFlow.Controls.Add(but_task);
            systemFlow.Controls.Add(but_user);
            systemFlow.Controls.Add(but_computer);

            //
            // cardThird：随程序自带的 Sysinternals 工具
            //
            cardThird.Controls.Add(thirdBody);
            cardThird.Description = "程序自带的 third_party 目录，不需要联网下载";
            cardThird.Dock = DockStyle.Fill;
            cardThird.Margin = new Padding(UiTheme.GapM, 0, 0, 0);
            cardThird.Name = "cardThird";
            cardThird.Title = "第三方工具";

            thirdBody.ColumnCount = 1;
            thirdBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            thirdBody.Controls.Add(thirdFlow, 0, 0);
            thirdBody.Controls.Add(lblThirdHint, 0, 1);
            thirdBody.Dock = DockStyle.Top;
            thirdBody.AutoSize = true;
            thirdBody.Name = "thirdBody";
            thirdBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapM, UiTheme.GapL, UiTheme.GapM);
            thirdBody.RowCount = 2;
            thirdBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            thirdBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            thirdFlow.Dock = DockStyle.Top;
            thirdFlow.AutoSize = true;
            thirdFlow.Name = "thirdFlow";

            Tool(but_autoruns, "but_autoruns", "Autoruns 启动项", "看开机自启的完整清单，比任务管理器里的全得多", but_autoruns_Click);
            Tool(but_procexp, "but_procexp", "Process Explorer", "比任务管理器细得多的进程树，能看句柄和 DLL", but_procexp_Click);

            thirdFlow.Controls.Add(but_autoruns);
            thirdFlow.Controls.Add(but_procexp);

            lblThirdHint.AutoSize = true;
            lblThirdHint.ForeColor = UiTheme.TextTertiary;
            lblThirdHint.Font = UiTheme.Small;
            lblThirdHint.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            lblThirdHint.MaximumSize = new Size(420, 0);
            lblThirdHint.Name = "lblThirdHint";
            lblThirdHint.Text = "这两个工具需要管理员权限才能看到全部信息；在 UAC 弹窗里点「是」即可。";

            //
            // statusRow：最近一次操作的结果，之前只写在 Console 里，界面根本看不见
            //
            statusRow.ColumnCount = 2;
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            statusRow.Controls.Add(pillAdmin, 0, 0);
            statusRow.Controls.Add(lblStatus, 1, 0);
            statusRow.Dock = DockStyle.Fill;
            statusRow.Name = "statusRow";
            statusRow.Padding = new Padding(0, UiTheme.GapL, 0, 0);
            statusRow.RowCount = 1;
            statusRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            pillAdmin.Margin = new Padding(0, 2, UiTheme.GapM, 0);
            pillAdmin.Name = "pillAdmin";
            pillAdmin.Text = "权限检测中";

            lblStatus.AutoSize = true;
            lblStatus.ForeColor = UiTheme.TextTertiary;
            lblStatus.Font = UiTheme.Small;
            lblStatus.Margin = new Padding(0, 4, 0, 0);
            lblStatus.MaximumSize = new Size(760, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "点上面的按钮打开对应工具，结果会显示在这里。";

            //
            // form_win_safe
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.WindowBackground;
            ClientSize = new Size(960, 460);
            Controls.Add(root);
            Font = UiTheme.Body;
            Name = "form_win_safe";
            Text = "windows 安全功能面板";

            statusRow.ResumeLayout(false);
            statusRow.PerformLayout();
            thirdFlow.ResumeLayout(false);
            thirdBody.ResumeLayout(false);
            thirdBody.PerformLayout();
            cardThird.ResumeLayout(false);
            systemFlow.ResumeLayout(false);
            cardSystem.ResumeLayout(false);
            body.ResumeLayout(false);
            root.ResumeLayout(false);
            ResumeLayout(false);
        }

        /// <summary>
        /// 设计器里生成按钮时顺手配好样式、可点区域和提示气泡，省得七个按钮抄七遍。
        /// </summary>
        private void Tool(FlatButton button, string name, string text, string tip, System.EventHandler handler)
        {
            button.Name = name;
            button.Text = text;
            button.Margin = new Padding(0, 0, UiTheme.GapS, UiTheme.GapS);
            button.Click += handler;
            toolTips.SetToolTip(button, tip);
        }

        #endregion

        private FlatButton but_evn;
        private FlatButton but_res_mon;
        private FlatButton but_task;
        private FlatButton but_user;
        private FlatButton but_computer;
        private FlatButton but_autoruns;
        private FlatButton but_procexp;
        private CardPanel cardSystem;
        private CardPanel cardThird;
        private PageHeader header;
        private TableLayoutPanel root;
        private TableLayoutPanel body;
        private TableLayoutPanel thirdBody;
        private TableLayoutPanel statusRow;
        private FlowLayoutPanel systemFlow;
        private FlowLayoutPanel thirdFlow;
        private Label lblThirdHint;
        private Label lblStatus;
        private StatusPill pillAdmin;
        private ToolTip toolTips = new ToolTip();
    }
}
