/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：MainForm.Designer.cs
 * 作者：zhenhua
 * 说明：主窗体的窗体设计器生成代码（侧边栏、品牌区、内容宿主、状态栏布局）。
 * 布局改动请优先在设计器里做，手写在这里会被下一次「生成设计器」覆盖。
 */
namespace WinDisplay
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources (mainly the window icon held by resources).
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            root = new TableLayoutPanel();
            sidebar = new Panel();
            navModules = new WinDisplay.Ui.NavList();
            brand = new Panel();
            lblBrandSub = new Label();
            brandGap = new Panel();
            lblBrand = new Label();
            navHint = new Label();
            contentHost = new Panel();
            statusBar = new TableLayoutPanel();
            leftFlow = new FlowLayoutPanel();
            pillAdmin = new WinDisplay.Ui.StatusPill();
            lblHint = new Label();
            rightFlow = new FlowLayoutPanel();
            lnkLog = new LinkLabel();
            lblVersion = new Label();
            root.SuspendLayout();
            sidebar.SuspendLayout();
            brand.SuspendLayout();
            statusBar.SuspendLayout();
            leftFlow.SuspendLayout();
            rightFlow.SuspendLayout();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 236F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(sidebar, 0, 0);
            root.Controls.Add(contentHost, 1, 0);
            root.Controls.Add(statusBar, 1, 1);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Name = "root";
            root.RowCount = 2;
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            root.Size = new Size(1180, 620);
            root.TabIndex = 0;
            // 
            // sidebar
            // 
            sidebar.BackColor = Color.FromArgb(27, 36, 48);
            sidebar.Controls.Add(navModules);
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(navHint);
            sidebar.Dock = DockStyle.Fill;
            sidebar.Location = new Point(3, 3);
            sidebar.Name = "sidebar";
            root.SetRowSpan(sidebar, 2);
            sidebar.Size = new Size(230, 614);
            sidebar.TabIndex = 0;
            // 
            // navModules
            // 
            navModules.BackColor = Color.FromArgb(27, 36, 48);
            navModules.Dock = DockStyle.Fill;
            navModules.Font = new Font("Microsoft YaHei UI", 9.5F);
            navModules.ForeColor = Color.FromArgb(220, 228, 238);
            navModules.Location = new Point(0, 78);
            navModules.Name = "navModules";
            navModules.SelectedIndex = -1;
            navModules.Size = new Size(230, 500);
            navModules.TabIndex = 1;
            // 
            // brand
            // 
            brand.BackColor = Color.FromArgb(27, 36, 48);
            brand.Controls.Add(lblBrandSub);
            brand.Controls.Add(brandGap);
            brand.Controls.Add(lblBrand);
            brand.Dock = DockStyle.Top;
            brand.Location = new Point(0, 0);
            brand.Name = "brand";
            brand.Padding = new Padding(20, 18, 12, 12);
            brand.Size = new Size(230, 78);
            brand.TabIndex = 0;
            // 
            // lblBrandSub
            // 
            lblBrandSub.AutoSize = true;
            lblBrandSub.BackColor = Color.Transparent;
            lblBrandSub.Dock = DockStyle.Top;
            lblBrandSub.Font = new Font("Microsoft YaHei UI", 8.25F);
            lblBrandSub.ForeColor = Color.FromArgb(159, 173, 190);
            lblBrandSub.Location = new Point(20, 49);
            lblBrandSub.Name = "lblBrandSub";
            lblBrandSub.Padding = new Padding(0, 3, 0, 0);
            lblBrandSub.Size = new Size(84, 19);
            lblBrandSub.TabIndex = 1;
            lblBrandSub.Text = "隐私与安全防护";
            // 
            // brandGap
            // 
            brandGap.BackColor = Color.Transparent;
            brandGap.Dock = DockStyle.Top;
            brandGap.Location = new Point(20, 43);
            brandGap.Name = "brandGap";
            brandGap.Size = new Size(198, 6);
            brandGap.TabIndex = 2;
            // 
            // lblBrand
            // 
            lblBrand.AutoSize = true;
            lblBrand.BackColor = Color.Transparent;
            lblBrand.Dock = DockStyle.Top;
            lblBrand.Font = new Font("Microsoft YaHei UI", 13.5F, FontStyle.Bold);
            lblBrand.ForeColor = Color.FromArgb(220, 228, 238);
            lblBrand.Location = new Point(20, 18);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(67, 25);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "x-保护";
            // 
            // navHint
            // 
            navHint.BackColor = Color.FromArgb(27, 36, 48);
            navHint.Dock = DockStyle.Bottom;
            navHint.Font = new Font("Microsoft YaHei UI", 8.25F);
            navHint.ForeColor = Color.FromArgb(159, 173, 190);
            navHint.Location = new Point(0, 578);
            navHint.Name = "navHint";
            navHint.Padding = new Padding(20, 6, 12, 12);
            navHint.Size = new Size(230, 36);
            navHint.TabIndex = 2;
            navHint.Text = "Ctrl + 1 ~ 5  快速切换模块";
            // 
            // contentHost
            // 
            contentHost.BackColor = Color.FromArgb(244, 246, 249);
            contentHost.Dock = DockStyle.Fill;
            contentHost.Location = new Point(239, 3);
            contentHost.Name = "contentHost";
            contentHost.Size = new Size(938, 582);
            contentHost.TabIndex = 1;
            // 
            // statusBar
            // 
            statusBar.BackColor = Color.White;
            statusBar.ColumnCount = 2;
            statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            statusBar.ColumnStyles.Add(new ColumnStyle());
            statusBar.Controls.Add(leftFlow, 0, 0);
            statusBar.Controls.Add(rightFlow, 1, 0);
            statusBar.Dock = DockStyle.Fill;
            statusBar.Location = new Point(239, 591);
            statusBar.Name = "statusBar";
            statusBar.Padding = new Padding(16, 0, 16, 0);
            statusBar.RowCount = 1;
            statusBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            statusBar.Size = new Size(938, 26);
            statusBar.TabIndex = 2;
            // 
            // leftFlow
            // 
            leftFlow.Controls.Add(pillAdmin);
            leftFlow.Controls.Add(lblHint);
            leftFlow.Dock = DockStyle.Fill;
            leftFlow.Location = new Point(19, 3);
            leftFlow.Name = "leftFlow";
            leftFlow.Padding = new Padding(0, 1, 0, 0);
            leftFlow.Size = new Size(590, 20);
            leftFlow.TabIndex = 0;
            // 
            // pillAdmin
            // 
            pillAdmin.BackColor = Color.FromArgb(247, 249, 252);
            pillAdmin.DotColor = Color.FromArgb(95, 110, 131);
            pillAdmin.FitText = true;
            pillAdmin.Font = new Font("Microsoft YaHei UI", 9F);
            pillAdmin.ForeColor = Color.FromArgb(75, 90, 110);
            pillAdmin.Location = new Point(0, 1);
            pillAdmin.Margin = new Padding(0, 0, 8, 0);
            pillAdmin.Name = "pillAdmin";
            pillAdmin.Size = new Size(98, 22);
            pillAdmin.TabIndex = 0;
            pillAdmin.Text = "权限检测中";
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.BackColor = Color.Transparent;
            lblHint.Font = new Font("Microsoft YaHei UI", 9F);
            lblHint.ForeColor = Color.FromArgb(95, 110, 131);
            lblHint.Location = new Point(116, 4);
            lblHint.Margin = new Padding(10, 3, 3, 0);
            lblHint.Name = "lblHint";
            lblHint.Padding = new Padding(0, 1, 0, 0);
            lblHint.Size = new Size(32, 18);
            lblHint.TabIndex = 1;
            lblHint.Text = "就绪";
            // 
            // rightFlow
            // 
            rightFlow.Controls.Add(lnkLog);
            rightFlow.Controls.Add(lblVersion);
            rightFlow.Dock = DockStyle.Fill;
            rightFlow.FlowDirection = FlowDirection.RightToLeft;
            rightFlow.Location = new Point(615, 3);
            rightFlow.Name = "rightFlow";
            rightFlow.Padding = new Padding(0, 4, 0, 0);
            rightFlow.Size = new Size(304, 20);
            rightFlow.TabIndex = 1;
            // 
            // lnkLog
            // 
            lnkLog.ActiveLinkColor = Color.FromArgb(29, 78, 216);
            lnkLog.AutoSize = true;
            lnkLog.Font = new Font("Microsoft YaHei UI", 9F);
            lnkLog.LinkColor = Color.FromArgb(29, 78, 216);
            lnkLog.Location = new Point(245, 7);
            lnkLog.Margin = new Padding(10, 3, 3, 0);
            lnkLog.Name = "lnkLog";
            lnkLog.Size = new Size(56, 17);
            lnkLog.TabIndex = 0;
            lnkLog.TabStop = true;
            lnkLog.Text = "打开日志";
            lnkLog.LinkClicked += LnkLog_LinkClicked;
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Microsoft YaHei UI", 9F);
            lblVersion.ForeColor = Color.FromArgb(95, 110, 131);
            lblVersion.Location = new Point(191, 7);
            lblVersion.Margin = new Padding(10, 3, 3, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Padding = new Padding(0, 1, 0, 0);
            lblVersion.Size = new Size(41, 18);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "版本 -";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(244, 246, 249);
            ClientSize = new Size(1180, 620);
            Controls.Add(root);
            Font = new Font("Microsoft YaHei UI", 9.5F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(940, 560);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "x-保护  (ver2.0)";
            KeyDown += MainForm_KeyDown;
            root.ResumeLayout(false);
            sidebar.ResumeLayout(false);
            brand.ResumeLayout(false);
            brand.PerformLayout();
            statusBar.ResumeLayout(false);
            leftFlow.ResumeLayout(false);
            leftFlow.PerformLayout();
            rightFlow.ResumeLayout(false);
            rightFlow.PerformLayout();
            ResumeLayout(false);
        }

        private TableLayoutPanel root;
        private Panel sidebar;
        private Panel brand;
        private Label lblBrand;
        private Panel brandGap;
        private Label lblBrandSub;
        private Label navHint;
        private Panel contentHost;
        private TableLayoutPanel statusBar;
        private FlowLayoutPanel leftFlow;
        private FlowLayoutPanel rightFlow;
        private Label lblHint;
        private LinkLabel lnkLog;
        private Label lblVersion;
        private WinDisplay.Ui.NavList navModules;
        private WinDisplay.Ui.StatusPill pillAdmin;

        #endregion
    }
}
