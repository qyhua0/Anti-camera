/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_clear.Designer.cs
 * 作者：zhenhua
 * 说明：「垃圾清理」模块的设计器生成代码（扫描结果 ListView、进度条、操作按钮）。
 */
using WinDisplay.Ui;

namespace WinDisplay
{
    partial class form_clear
    {
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel body;
        private PageHeader header;
        private CardPanel cardActions;
        private CardPanel cardResults;
        private System.Windows.Forms.FlowLayoutPanel actionFlow;
        private FlatButton btnScan;
        private FlatButton btnClean;
        private FlatButton btnSelectAll;
        private FlatButton btnDeselectAll;
        private Label lblTotalSize;
        private StatusPill pillPermission;
        private System.Windows.Forms.TableLayoutPanel resultBody;
        private System.Windows.Forms.TableLayoutPanel resultFoot;
        private ListView listViewResults;
        private UiProgressBar progressBar1;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                listViewResults?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            body = new System.Windows.Forms.TableLayoutPanel();
            cardActions = new CardPanel();
            actionFlow = new System.Windows.Forms.FlowLayoutPanel();
            btnScan = new FlatButton();
            btnClean = new FlatButton();
            btnSelectAll = new FlatButton();
            btnDeselectAll = new FlatButton();
            lblTotalSize = new Label();
            pillPermission = new StatusPill();
            cardResults = new CardPanel();
            resultBody = new System.Windows.Forms.TableLayoutPanel();
            listViewResults = new ListView();
            resultFoot = new System.Windows.Forms.TableLayoutPanel();
            progressBar1 = new UiProgressBar();
            lblStatus = new Label();
            header = new PageHeader();
            root.SuspendLayout();
            body.SuspendLayout();
            cardActions.SuspendLayout();
            actionFlow.SuspendLayout();
            cardResults.SuspendLayout();
            resultBody.SuspendLayout();
            resultFoot.SuspendLayout();
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
            root.Padding = new Padding(UiTheme.PagePadding, UiTheme.PagePadding, UiTheme.PagePadding, UiTheme.PagePadding);
            root.RowCount = 2;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 
            // header
            // 
            header.Description = "只删除扫描到的位置里、符合该项文件类型的文件；灰色项目是只读参考，要按提示手动处理。";
            header.Dock = DockStyle.Fill;
            header.Name = "header";
            header.Title = "垃圾清理";
            // 
            // body
            // 
            body.ColumnCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Controls.Add(cardActions, 0, 0);
            body.Controls.Add(cardResults, 0, 1);
            body.Dock = DockStyle.Fill;
            body.Name = "body";
            body.RowCount = 2;
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 
            // cardActions
            // 
            cardActions.Controls.Add(actionFlow);
            cardActions.Dock = DockStyle.Fill;
            cardActions.Margin = new Padding(0, 0, 0, UiTheme.GapM);
            cardActions.Name = "cardActions";
            cardActions.Title = "操作";
            // 
            // actionFlow
            // 
            actionFlow.AutoSize = true;
            actionFlow.Controls.Add(btnScan);
            actionFlow.Controls.Add(btnClean);
            actionFlow.Controls.Add(btnSelectAll);
            actionFlow.Controls.Add(btnDeselectAll);
            actionFlow.Controls.Add(lblTotalSize);
            actionFlow.Controls.Add(pillPermission);
            actionFlow.Dock = DockStyle.Top;
            actionFlow.Name = "actionFlow";
            // 
            // btnScan
            // 
            btnScan.Name = "btnScan";
            btnScan.Text = "扫描垃圾文件";
            btnScan.Variant = ButtonVariant.Primary;
            // 
            // btnClean
            // 
            btnClean.Name = "btnClean";
            btnClean.Text = "清理选中项";
            btnClean.Variant = ButtonVariant.Danger;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Text = "全部勾选";
            btnSelectAll.Variant = ButtonVariant.Ghost;
            // 
            // btnDeselectAll
            // 
            btnDeselectAll.Name = "btnDeselectAll";
            btnDeselectAll.Text = "全部取消";
            btnDeselectAll.Variant = ButtonVariant.Ghost;
            // 
            // lblTotalSize
            // 
            lblTotalSize.AutoSize = true;
            lblTotalSize.Font = UiTheme.BodyBold;
            lblTotalSize.ForeColor = UiTheme.TextSecondary;
            lblTotalSize.Margin = new Padding(0, 8, 0, 0);
            lblTotalSize.Name = "lblTotalSize";
            lblTotalSize.Text = "0 MB";
            // 
            // pillPermission
            // 
            pillPermission.DotColor = UiTheme.TextTertiary;
            pillPermission.Name = "pillPermission";
            pillPermission.Text = "检测权限中";
            // 
            // cardResults
            // 
            cardResults.Controls.Add(resultBody);
            cardResults.Dock = DockStyle.Fill;
            cardResults.Name = "cardResults";
            cardResults.Title = "扫描结果";
            // 
            // resultBody
            // 
            resultBody.ColumnCount = 1;
            resultBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            resultBody.Controls.Add(listViewResults, 0, 0);
            resultBody.Controls.Add(resultFoot, 0, 1);
            resultBody.Dock = DockStyle.Fill;
            resultBody.Name = "resultBody";
            resultBody.RowCount = 2;
            resultBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            resultBody.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // 
            // listViewResults
            // 
            listViewResults.Dock = DockStyle.Fill;
            listViewResults.Name = "listViewResults";
            UiTheme.ApplyListView(listViewResults);
            // 
            // resultFoot
            // 
            resultFoot.ColumnCount = 2;
            resultFoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            resultFoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            resultFoot.Controls.Add(progressBar1, 0, 0);
            resultFoot.Controls.Add(lblStatus, 1, 0);
            resultFoot.Dock = DockStyle.Top;
            resultFoot.AutoSize = true;
            resultFoot.Name = "resultFoot";
            resultFoot.Padding = new Padding(0, UiTheme.GapM, 0, 0);
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Fill;
            progressBar1.Maximum = 100;
            progressBar1.Name = "progressBar1";
            progressBar1.Height = 10;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = UiTheme.Small;
            lblStatus.ForeColor = UiTheme.TextSecondary;
            lblStatus.Margin = new Padding(UiTheme.GapM, 0, 0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "请扫描";
            // 
            // form_clear
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.WindowBackground;
            ClientSize = new Size(1140, 520);
            Controls.Add(root);
            Font = UiTheme.Body;
            MinimumSize = new Size(760, 420);
            Name = "form_clear";
            Text = "垃圾清理";
            Load += form_clear_Load;
            root.ResumeLayout(false);
            body.ResumeLayout(false);
            cardActions.ResumeLayout(false);
            actionFlow.ResumeLayout(false);
            actionFlow.PerformLayout();
            cardResults.ResumeLayout(false);
            resultBody.ResumeLayout(false);
            resultFoot.ResumeLayout(false);
            resultFoot.PerformLayout();
            ResumeLayout(false);
        }
    }
}
