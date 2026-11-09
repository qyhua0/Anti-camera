/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_screen.Designer.cs
 * 作者：zhenhua
 * 说明：「屏幕防拍摄」模块的设计器生成代码：左列亮度/实验卡片、右列遮罩卡片，
 * 滚动条挂在列容器 leftCol/rightCol 上（挂在有 Percent 列的 body 上会导致列宽不重排、右侧被切）。
 */
using WinDisplay.Ui;

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

            root = new TableLayoutPanel();
            header = new PageHeader();
            body = new TableLayoutPanel();
            leftCol = new TableLayoutPanel();
            rightCol = new TableLayoutPanel();
            cardBright = new CardPanel();
            brightBody = new TableLayoutPanel();
            brightStats = new FlowLayoutPanel();
            metricBrightness = new MetricView();
            pillGamma = new StatusPill();
            pillOverlay = new StatusPill();
            trackBar_bright = new TrackBar();
            presetFlow = new FlowLayoutPanel();
            but_bright50 = new FlatButton();
            but_bright60 = new FlatButton();
            but_bright70 = new FlatButton();
            but_bright80 = new FlatButton();
            but_bright90 = new FlatButton();
            but_brightMax = new FlatButton();
            lblBrightHint = new Label();
            cardLab = new CardPanel();
            labBody = new FlowLayoutPanel();
            but_high_frequency = new FlatButton();
            lblLabHint = new Label();
            cardMask = new CardPanel();
            maskBody = new TableLayoutPanel();
            label2 = new Label();
            txt_maskform_tip = new TextBox();
            opacityRow = new FlowLayoutPanel();
            label1 = new Label();
            opt_apla1 = new NumericUpDown();
            lblOpacityHint = new Label();
            maskFlow = new FlowLayoutPanel();
            but_maskWin = new FlatButton();
            but_hide = new FlatButton();
            but_mask_lock = new FlatButton();
            positionRow = new TableLayoutPanel();
            but_post_up = new FlatButton();
            but_post_left = new FlatButton();
            but_post_right = new FlatButton();
            but_post_down = new FlatButton();
            lblPositionHint = new Label();
            pillMask = new StatusPill();

            root.SuspendLayout();
            body.SuspendLayout();
            leftCol.SuspendLayout();
            rightCol.SuspendLayout();
            cardBright.SuspendLayout();
            brightBody.SuspendLayout();
            brightStats.SuspendLayout();
            presetFlow.SuspendLayout();
            cardLab.SuspendLayout();
            labBody.SuspendLayout();
            cardMask.SuspendLayout();
            maskBody.SuspendLayout();
            opacityRow.SuspendLayout();
            maskFlow.SuspendLayout();
            positionRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opt_apla1).BeginInit();
            SuspendLayout();

            //
            // root：上「页头」下「内容」，模块被嵌进主窗体的内容区，所以自身只负责这一页
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
            header.Description = "先压暗屏幕，再用挡板盖住关键区域；退出程序会自动把亮度还原。";
            header.Dock = DockStyle.Fill;
            header.Name = "header";
            header.Title = "屏幕防拍摄";

            //
            // body：左列（亮度 + 实验功能）竖排，右列挡板；内容比窗口高时整块可滚动
            //
            body.ColumnCount = 2;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            body.Controls.Add(leftCol, 0, 0);
            body.Controls.Add(rightCol, 1, 0);
            body.Dock = DockStyle.Fill;
            body.Name = "body";
            body.RowCount = 1;
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            //
            // leftCol：亮度卡片在上、实验功能卡片在下，两行都按内容高
            //
            leftCol.Controls.Add(cardBright);
            leftCol.Controls.Add(cardLab);
            leftCol.ColumnCount = 1;
            leftCol.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftCol.Dock = DockStyle.Fill;
            // 滚动条只吃这一列的宽度，列宽本身不变，不会出现滚动条出现后右边被切掉
            leftCol.AutoScroll = true;
            leftCol.Name = "leftCol";
            leftCol.RowCount = 2;
            leftCol.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftCol.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            //
            // rightCol：挡板卡片。用 Dock=Fill 塞进一个 AutoSize 行，
            // 不用 Anchor——Anchor 在滚动条出现、列宽变窄后不会重新排版，卡片右边会被切掉。
            //
            rightCol.Controls.Add(cardMask);
            rightCol.ColumnCount = 1;
            rightCol.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rightCol.Dock = DockStyle.Fill;
            rightCol.AutoScroll = true;
            rightCol.Name = "rightCol";
            rightCol.RowCount = 1;
            rightCol.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            //
            // cardBright
            //
            cardBright.Controls.Add(brightBody);
            cardBright.Description = "显卡伽马 + 全屏半透明遮罩两级减光";
            cardBright.Dock = DockStyle.Fill;
            cardBright.Margin = new Padding(0, 0, UiTheme.GapM, UiTheme.GapM);
            cardBright.Name = "cardBright";
            cardBright.Title = "屏幕亮度";

            brightBody.ColumnCount = 1;
            brightBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            brightBody.Controls.Add(metricBrightness, 0, 0);
            brightBody.Controls.Add(brightStats, 0, 1);
            brightBody.Controls.Add(trackBar_bright, 0, 2);
            brightBody.Controls.Add(presetFlow, 0, 3);
            brightBody.Controls.Add(lblBrightHint, 0, 4);
            brightBody.Dock = DockStyle.Top;
            brightBody.AutoSize = true;
            brightBody.Name = "brightBody";
            brightBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            brightBody.RowCount = 5;
            for (int i = 0; i < 5; i++)
            {
                brightBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            //
            // brightStats：大号当前值 + 伽马通道状态
            //
            brightStats.AutoSize = true;
            brightStats.Dock = DockStyle.Top;
            brightStats.Name = "brightStats";
            brightStats.Margin = new Padding(0, 0, 0, UiTheme.GapS);
            brightStats.Controls.Add(pillGamma);
            brightStats.Controls.Add(pillOverlay);

            metricBrightness.Caption = "当前亮度";
            metricBrightness.Margin = new Padding(0, 0, 0, 0);
            metricBrightness.Name = "metricBrightness";
            metricBrightness.Unit = "%";
            metricBrightness.Value = "100";

            pillGamma.Name = "pillGamma";
            pillGamma.Margin = new Padding(0, UiTheme.GapM, UiTheme.GapS, 0);
            pillGamma.SetState("检测显卡中", UiTheme.TextTertiary, UiTheme.SurfaceMuted, UiTheme.TextSecondary);

            pillOverlay.Name = "pillOverlay";
            pillOverlay.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            pillOverlay.SetState("未使用遮罩", UiTheme.TextTertiary, UiTheme.SurfaceMuted, UiTheme.TextSecondary);

            //
            // trackBar_bright
            //
            trackBar_bright.Dock = DockStyle.Top;
            trackBar_bright.Maximum = 100;
            trackBar_bright.Minimum = 1;
            trackBar_bright.Name = "trackBar_bright";
            trackBar_bright.SmallChange = 5;
            trackBar_bright.LargeChange = 10;
            trackBar_bright.Height = 42;
            trackBar_bright.TickFrequency = 10;
            trackBar_bright.Value = 100;
            trackBar_bright.Scroll += trackBar_bright_Scroll;

            //
            // presetFlow：常用档位。Tag 里放数字，统一由 but_brightPreset_Click 处理
            //
            presetFlow.AutoSize = true;
            presetFlow.Dock = DockStyle.Top;
            presetFlow.Name = "presetFlow";
            presetFlow.Padding = new Padding(0, UiTheme.GapS, 0, 0);

            // 共用属性用局部函数收一下，既能捕到 this 挂事件，又不用写五遍
            void Preset(FlatButton button, string brightness)
            {
                button.Name = "but_bright" + brightness;
                button.Tag = brightness;
                button.Text = brightness + "%";
                button.Click += but_brightPreset_Click;
            }

            Preset(but_bright50, "50");
            Preset(but_bright60, "60");
            Preset(but_bright70, "70");
            Preset(but_bright80, "80");
            Preset(but_bright90, "90");

            but_brightMax.Name = "but_brightMax";
            but_brightMax.Tag = "100";
            but_brightMax.Text = "恢复 100%";
            but_brightMax.Variant = ButtonVariant.Primary;
            but_brightMax.Click += but_brightPreset_Click;

            presetFlow.Controls.Add(but_bright50);
            presetFlow.Controls.Add(but_bright60);
            presetFlow.Controls.Add(but_bright70);
            presetFlow.Controls.Add(but_bright80);
            presetFlow.Controls.Add(but_bright90);
            presetFlow.Controls.Add(but_brightMax);

            lblBrightHint.Dock = DockStyle.Top;
            lblBrightHint.ForeColor = UiTheme.TextTertiary;
            lblBrightHint.Font = UiTheme.Small;
            lblBrightHint.MaximumSize = new Size(0, 200);   // 限制高度不生效前先限宽，让它按宽度折行
            lblBrightHint.Name = "lblBrightHint";
            lblBrightHint.Padding = new Padding(0, UiTheme.GapM, 0, 0);
            lblBrightHint.Text = "显卡驱动只允许伽马曲线偏移一半左右，压过头的那部分会自动交给全屏半透明遮罩，所以滑到 20% 就拿得到 20%。拖得太低看不清时，点「恢复 100%」或托盘菜单的「恢复亮度」。";
            lblBrightHint.AutoSize = true;

            //
            // cardLab：实验功能
            //
            cardLab.Controls.Add(labBody);
            cardLab.Description = "高频闪烁：画面会快速明暗切换，光敏人群慎用";
            cardLab.Dock = DockStyle.Fill;
            cardLab.Margin = new Padding(0, 0, UiTheme.GapM, 0);
            cardLab.Name = "cardLab";
            cardLab.Title = "实验功能";

            labBody.Dock = DockStyle.Top;
            labBody.AutoSize = true;
            labBody.Name = "labBody";
            labBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            labBody.WrapContents = true;

            but_high_frequency.Name = "but_high_frequency";
            but_high_frequency.Text = "启动高频闪烁";
            but_high_frequency.Variant = ButtonVariant.Danger;
            but_high_frequency.Click += but_high_frequency_Click;

            lblLabHint.AutoSize = true;
            lblLabHint.ForeColor = UiTheme.TextTertiary;
            lblLabHint.Font = UiTheme.Small;
            lblLabHint.Margin = new Padding(UiTheme.GapS, UiTheme.GapM, 0, 0);
            lblLabHint.MaximumSize = new Size(300, 0);
            lblLabHint.Name = "lblLabHint";
            lblLabHint.Text = "闪屏窗体按 Esc 关闭。频繁明暗切换可能引起不适。";

            labBody.Controls.Add(but_high_frequency);
            labBody.Controls.Add(lblLabHint);

            //
            // cardMask
            //
            cardMask.Controls.Add(maskBody);
            cardMask.Description = "在屏幕上加一块不透明区域，可锁定防止拖动";
            cardMask.Dock = DockStyle.Fill;
            cardMask.Name = "cardMask";
            cardMask.Title = "隐私挡板";
            cardMask.HeaderAction = pillMask;

            maskBody.ColumnCount = 1;
            maskBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            maskBody.Controls.Add(label2, 0, 0);
            maskBody.Controls.Add(txt_maskform_tip, 0, 1);
            maskBody.Controls.Add(opacityRow, 0, 2);
            maskBody.Controls.Add(maskFlow, 0, 3);
            maskBody.Controls.Add(positionRow, 0, 4);
            maskBody.Controls.Add(lblPositionHint, 0, 5);
            maskBody.Dock = DockStyle.Top;
            maskBody.AutoSize = true;
            maskBody.Name = "maskBody";
            maskBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            maskBody.RowCount = 6;
            for (int i = 0; i < 5; i++)
            {
                maskBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            maskBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            label2.AutoSize = true;
            label2.ForeColor = UiTheme.TextSecondary;
            label2.Name = "label2";
            label2.Text = "挡板上的提示文字";

            txt_maskform_tip.Font = UiTheme.Body;
            txt_maskform_tip.Multiline = true;
            txt_maskform_tip.Name = "txt_maskform_tip";
            txt_maskform_tip.Height = 48;
            txt_maskform_tip.Dock = DockStyle.Top;
            txt_maskform_tip.Margin = new Padding(0, UiTheme.GapXS, 0, 0);
            txt_maskform_tip.Text = "保护隐私,禁止偷看";

            opacityRow.AutoSize = true;
            opacityRow.Dock = DockStyle.Top;
            opacityRow.Name = "opacityRow";
            opacityRow.Padding = new Padding(0, UiTheme.GapM, 0, 0);

            label1.AutoSize = true;
            label1.ForeColor = UiTheme.TextSecondary;
            label1.Margin = new Padding(0, UiTheme.GapS, UiTheme.GapS, 0);
            label1.Name = "label1";
            label1.Text = "不透光度";

            opt_apla1.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            opt_apla1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            opt_apla1.Margin = new Padding(0, UiTheme.GapXS, 0, 0);
            opt_apla1.Name = "opt_apla1";
            opt_apla1.Width = 64;
            opt_apla1.Value = new decimal(new int[] { 5, 0, 0, 0 });

            lblOpacityHint.AutoSize = true;
            lblOpacityHint.ForeColor = UiTheme.TextTertiary;
            lblOpacityHint.Font = UiTheme.Small;
            lblOpacityHint.Margin = new Padding(UiTheme.GapS, UiTheme.GapM, 0, 0);
            lblOpacityHint.Name = "lblOpacityHint";
            lblOpacityHint.Text = "1 几乎透明 · 10 完全遮挡";

            opacityRow.Controls.Add(label1);
            opacityRow.Controls.Add(opt_apla1);
            opacityRow.Controls.Add(lblOpacityHint);

            maskFlow.AutoSize = true;
            maskFlow.Dock = DockStyle.Top;
            maskFlow.Name = "maskFlow";
            maskFlow.Padding = new Padding(0, UiTheme.GapL, 0, 0);

            but_maskWin.Name = "but_maskWin";
            but_maskWin.Text = "添加挡板";
            but_maskWin.Variant = ButtonVariant.Primary;
            but_maskWin.Click += but_maskWin_Click;

            but_hide.Name = "but_hide";
            but_hide.Text = "隐藏挡板";
            but_hide.Click += but_hide_Click;

            but_mask_lock.Name = "but_mask_lock";
            but_mask_lock.Text = "固定挡板";
            but_mask_lock.Click += but_mask_lock_Click;

            maskFlow.Controls.Add(but_maskWin);
            maskFlow.Controls.Add(but_hide);
            maskFlow.Controls.Add(but_mask_lock);

            // 方向键摆成九宫格，比一排「上下左右」直观
            positionRow.ColumnCount = 3;
            positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74F));
            positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74F));
            positionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74F));
            positionRow.Controls.Add(but_post_up, 1, 0);
            positionRow.Controls.Add(but_post_left, 0, 1);
            positionRow.Controls.Add(but_post_right, 2, 1);
            positionRow.Controls.Add(but_post_down, 1, 2);
            positionRow.Dock = DockStyle.Top;
            positionRow.Name = "positionRow";
            positionRow.Padding = new Padding(0, UiTheme.GapL, 0, 0);
            positionRow.AutoSize = true;
            positionRow.RowCount = 3;
            for (int i = 0; i < 3; i++)
            {
                positionRow.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            void Position(FlatButton button, string name, string text, System.EventHandler handler)
            {
                button.Name = name;
                button.Text = text;
                button.Margin = new Padding(0);
                button.Click += handler;
            }

            Position(but_post_up, "but_post_up", "上", but_post_up_Click);
            Position(but_post_left, "but_post_left", "左", but_post_left_Click);
            Position(but_post_right, "but_post_right", "右", but_post_right_Click);
            Position(but_post_down, "but_post_down", "下", but_post_down_Click);

            lblPositionHint.AutoSize = true;
            lblPositionHint.ForeColor = UiTheme.TextTertiary;
            lblPositionHint.Font = UiTheme.Small;
            lblPositionHint.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            lblPositionHint.MaximumSize = new Size(280, 0);
            lblPositionHint.Name = "lblPositionHint";
            lblPositionHint.Text = "把最后一块挡板移到对应位置。挡板锁定后不能拖动，只能在这里挪。";
            lblPositionHint.Dock = DockStyle.Top;

            //
            // frm_screen
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.WindowBackground;
            ClientSize = new Size(880, 520);
            Controls.Add(root);
            Font = UiTheme.Body;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frm_screen";
            Text = "屏幕保护";

            ((System.ComponentModel.ISupportInitialize)opt_apla1).EndInit();
            positionRow.ResumeLayout(false);
            maskFlow.ResumeLayout(false);
            opacityRow.ResumeLayout(false);
            maskBody.ResumeLayout(false);
            cardMask.ResumeLayout(false);
            labBody.ResumeLayout(false);
            cardLab.ResumeLayout(false);
            presetFlow.ResumeLayout(false);
            brightStats.ResumeLayout(false);
            brightBody.ResumeLayout(false);
            cardBright.ResumeLayout(false);
            leftCol.ResumeLayout(false);
            rightCol.ResumeLayout(false);
            body.ResumeLayout(false);
            root.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlatButton but_bright50;
        private FlatButton but_bright60;
        private FlatButton but_bright70;
        private FlatButton but_bright80;
        private FlatButton but_bright90;
        private FlatButton but_brightMax;
        private TrackBar trackBar_bright;
        private FlatButton but_maskWin;
        private FlatButton but_mask_lock;
        private FlatButton but_high_frequency;
        private FlatButton but_hide;
        private FlatButton but_post_left;
        private FlatButton but_post_down;
        private FlatButton but_post_up;
        private FlatButton but_post_right;
        private Label label1;
        private Label label2;
        private Label lblBrightHint;
        private Label lblOpacityHint;
        private Label lblPositionHint;
        private Label lblLabHint;
        private NumericUpDown opt_apla1;
        private TextBox txt_maskform_tip;
        private CardPanel cardBright;
        private CardPanel cardMask;
        private CardPanel cardLab;
        private PageHeader header;
        private MetricView metricBrightness;
        private StatusPill pillGamma;
        private StatusPill pillOverlay;
        private StatusPill pillMask;
        private TableLayoutPanel root;
        private TableLayoutPanel body;
        private TableLayoutPanel leftCol;
        private TableLayoutPanel rightCol;
        private TableLayoutPanel brightBody;
        private TableLayoutPanel maskBody;
        private TableLayoutPanel positionRow;
        private FlowLayoutPanel brightStats;
        private FlowLayoutPanel presetFlow;
        private FlowLayoutPanel maskFlow;
        private FlowLayoutPanel opacityRow;
        private FlowLayoutPanel labBody;
    }
}
