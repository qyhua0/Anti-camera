/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_safe_remove.Designer.cs
 * 作者：zhenhua
 * 说明：「文件粉碎」模块的设计器生成代码（文件选择、擦除强度、进度显示）。
 */
using WinDisplay.Ui;

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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            root = new TableLayoutPanel();
            header = new PageHeader();
            body = new TableLayoutPanel();
            cardFile = new CardPanel();
            fileBody = new TableLayoutPanel();
            txtPath = new TextBox();
            but_selFile = new FlatButton();
            cardOptions = new CardPanel();
            optionBody = new TableLayoutPanel();
            optFlow = new FlowLayoutPanel();
            label2 = new Label();
            num_count = new NumericUpDown();
            lblCountHint = new Label();
            zeroFlow = new FlowLayoutPanel();
            cb_fillZero = new ToggleSwitch();
            lblZeroHint = new Label();
            lblDanger = new Label();
            actionFlow = new FlowLayoutPanel();
            but_exec = new FlatButton();
            lblActionHint = new Label();
            cardProgress = new CardPanel();
            progressBody = new TableLayoutPanel();
            progressEraser = new UiProgressBar();
            lblProgressHint = new Label();
            pillState = new StatusPill();

            root.SuspendLayout();
            body.SuspendLayout();
            cardFile.SuspendLayout();
            fileBody.SuspendLayout();
            cardOptions.SuspendLayout();
            optionBody.SuspendLayout();
            optFlow.SuspendLayout();
            zeroFlow.SuspendLayout();
            cardProgress.SuspendLayout();
            progressBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_count).BeginInit();
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
            header.Description = "用随机数据反复覆写文件内容后再删除，让恢复工具也拼不回来。";
            header.Dock = DockStyle.Fill;
            header.Name = "header";
            header.Title = "文件粉碎";

            //
            // body：三张卡片从上到下，最后一行留白，别把卡片拉成一条
            //
            body.ColumnCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Controls.Add(cardFile, 0, 0);
            body.Controls.Add(cardOptions, 0, 1);
            body.Controls.Add(cardProgress, 0, 2);
            body.Dock = DockStyle.Fill;
            body.Name = "body";
            body.RowCount = 4;
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            //
            // cardFile
            //
            cardFile.Controls.Add(fileBody);
            cardFile.Description = "一次一个文件；要粉碎整个目录，请先压成一个压缩包";
            cardFile.Dock = DockStyle.Fill;
            cardFile.Name = "cardFile";
            cardFile.Title = "选择文件";

            fileBody.ColumnCount = 2;
            fileBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fileBody.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
            fileBody.Controls.Add(txtPath, 0, 0);
            fileBody.Controls.Add(but_selFile, 1, 0);
            fileBody.Dock = DockStyle.Top;
            fileBody.AutoSize = true;
            fileBody.Name = "fileBody";
            fileBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            fileBody.RowCount = 1;
            fileBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            txtPath.Dock = DockStyle.Fill;
            txtPath.Name = "txtPath";
            txtPath.PlaceholderText = "还没选择文件";
            txtPath.Margin = new Padding(0, UiTheme.GapXS, UiTheme.GapS, 0);

            but_selFile.Dock = DockStyle.Fill;
            but_selFile.Margin = new Padding(0, UiTheme.GapXS, 0, 0);
            but_selFile.Name = "but_selFile";
            but_selFile.Text = "浏览…";
            but_selFile.Click += but_selFile_Click;

            //
            // cardOptions
            //
            cardOptions.Controls.Add(optionBody);
            cardOptions.Description = "遍数越多越慢，安全性不会线性提升";
            cardOptions.Dock = DockStyle.Fill;
            cardOptions.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            cardOptions.Name = "cardOptions";
            cardOptions.Title = "覆写选项";

            optionBody.ColumnCount = 1;
            optionBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            optionBody.Controls.Add(optFlow, 0, 0);
            optionBody.Controls.Add(zeroFlow, 0, 1);
            optionBody.Controls.Add(lblDanger, 0, 2);
            optionBody.Controls.Add(actionFlow, 0, 3);
            optionBody.Dock = DockStyle.Top;
            optionBody.AutoSize = true;
            optionBody.Name = "optionBody";
            optionBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            optionBody.RowCount = 4;
            for (int i = 0; i < 4; i++)
            {
                optionBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            optFlow.AutoSize = true;
            optFlow.Name = "optFlow";

            label2.AutoSize = true;
            label2.ForeColor = UiTheme.TextSecondary;
            label2.Margin = new Padding(0, UiTheme.GapS, UiTheme.GapS, 0);
            label2.Name = "label2";
            label2.Text = "覆盖次数";

            num_count.Margin = new Padding(0, UiTheme.GapXS, 0, 0);
            num_count.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            // 上限从 1000 降到 10：每一遍都要重写整个文件，1000 遍等于把盘写坏风险拉满而安全收益为零
            num_count.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            num_count.Name = "num_count";
            num_count.Width = 72;
            num_count.Value = new decimal(new int[] { 3, 0, 0, 0 });

            lblCountHint.AutoSize = true;
            lblCountHint.ForeColor = UiTheme.TextTertiary;
            lblCountHint.Font = UiTheme.Small;
            lblCountHint.Margin = new Padding(UiTheme.GapS, UiTheme.GapM, 0, 0);
            lblCountHint.Name = "lblCountHint";
            lblCountHint.Text = "1 ~ 10 次，默认 3 次";

            optFlow.Controls.Add(label2);
            optFlow.Controls.Add(num_count);
            optFlow.Controls.Add(lblCountHint);

            zeroFlow.AutoSize = true;
            zeroFlow.Name = "zeroFlow";

            cb_fillZero.Checked = true;
            cb_fillZero.CheckState = CheckState.Checked;
            cb_fillZero.Margin = new Padding(0, UiTheme.GapM, UiTheme.GapS, 0);
            cb_fillZero.Name = "cb_fillZero";
            cb_fillZero.Text = "最后写一遍 0";

            lblZeroHint.AutoSize = true;
            lblZeroHint.ForeColor = UiTheme.TextTertiary;
            lblZeroHint.Font = UiTheme.Small;
            lblZeroHint.Margin = new Padding(0, UiTheme.GapM + UiTheme.GapXS, 0, 0);
            lblZeroHint.Name = "lblZeroHint";
            lblZeroHint.Text = "覆写完成后文件内容全为零，看起来像被清空过";

            zeroFlow.Controls.Add(cb_fillZero);
            zeroFlow.Controls.Add(lblZeroHint);

            lblDanger.AutoSize = true;
            lblDanger.ForeColor = UiTheme.DangerText;
            lblDanger.Font = UiTheme.Small;
            lblDanger.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            lblDanger.MaximumSize = new Size(520, 0);
            lblDanger.Name = "lblDanger";
            lblDanger.Text = "覆写无法撤销：文件一旦开始处理就找不回来，请先确认路径。";

            actionFlow.AutoSize = true;
            actionFlow.Name = "actionFlow";
            actionFlow.Padding = new Padding(0, UiTheme.GapL, 0, 0);

            but_exec.Name = "but_exec";
            but_exec.Text = "开始粉碎";
            but_exec.Variant = ButtonVariant.Danger;
            but_exec.Click += button1_Click;

            lblActionHint.AutoSize = true;
            lblActionHint.ForeColor = UiTheme.TextTertiary;
            lblActionHint.Font = UiTheme.Small;
            lblActionHint.Margin = new Padding(UiTheme.GapM, UiTheme.GapM, 0, 0);
            lblActionHint.Name = "lblActionHint";
            lblActionHint.Text = "执行前会再确认一次";

            actionFlow.Controls.Add(but_exec);
            actionFlow.Controls.Add(lblActionHint);

            //
            // cardProgress
            //
            cardProgress.Controls.Add(progressBody);
            cardProgress.Description = "大文件要跑一会儿，进度实时更新";
            cardProgress.Dock = DockStyle.Fill;
            cardProgress.HeaderAction = pillState;
            cardProgress.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            cardProgress.Name = "cardProgress";
            cardProgress.Title = "进度";

            progressBody.ColumnCount = 1;
            progressBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            progressBody.Controls.Add(progressEraser, 0, 0);
            progressBody.Controls.Add(lblProgressHint, 0, 1);
            progressBody.Dock = DockStyle.Top;
            progressBody.AutoSize = true;
            progressBody.Name = "progressBody";
            progressBody.Padding = new Padding(UiTheme.GapL, UiTheme.GapS, UiTheme.GapL, UiTheme.GapM);
            progressBody.RowCount = 2;
            progressBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            progressBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            progressEraser.Dock = DockStyle.Top;
            progressEraser.Name = "progressEraser";

            lblProgressHint.AutoSize = true;
            lblProgressHint.ForeColor = UiTheme.TextTertiary;
            lblProgressHint.Font = UiTheme.Small;
            lblProgressHint.Margin = new Padding(0, UiTheme.GapM, 0, 0);
            lblProgressHint.Name = "lblProgressHint";
            lblProgressHint.Text = "擦除期间不要关闭程序，也不要拔掉 U 盘。";

            //
            // form_safe_remove
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.WindowBackground;
            ClientSize = new Size(760, 420);
            Controls.Add(root);
            Font = UiTheme.Body;
            Name = "form_safe_remove";
            Text = "文件清除";

            ((System.ComponentModel.ISupportInitialize)num_count).EndInit();
            progressBody.ResumeLayout(false);
            progressBody.PerformLayout();
            cardProgress.ResumeLayout(false);
            zeroFlow.ResumeLayout(false);
            zeroFlow.PerformLayout();
            optFlow.ResumeLayout(false);
            optFlow.PerformLayout();
            optionBody.ResumeLayout(false);
            optionBody.PerformLayout();
            cardOptions.ResumeLayout(false);
            fileBody.ResumeLayout(false);
            fileBody.PerformLayout();
            cardFile.ResumeLayout(false);
            body.ResumeLayout(false);
            root.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtPath;
        private FlatButton but_exec;
        private Label lblActionHint;
        private FlowLayoutPanel actionFlow;
        private FlatButton but_selFile;
        private ToggleSwitch cb_fillZero;
        private Label label2;
        private NumericUpDown num_count;
        private Label lblCountHint;
        private Label lblZeroHint;
        private Label lblDanger;
        private Label lblProgressHint;
        private UiProgressBar progressEraser;
        private StatusPill pillState;
        private CardPanel cardFile;
        private CardPanel cardOptions;
        private CardPanel cardProgress;
        private PageHeader header;
        private TableLayoutPanel root;
        private TableLayoutPanel body;
        private TableLayoutPanel fileBody;
        private TableLayoutPanel optionBody;
        private TableLayoutPanel progressBody;
        private FlowLayoutPanel optFlow;
        private FlowLayoutPanel zeroFlow;
    }
}
