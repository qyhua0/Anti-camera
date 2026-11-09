/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：Form_net.Designer.cs
 * 作者：zhenhua
 * 说明：「网络流量分析」模块的设计器生成代码（网卡下拉、开始/停止按钮、连接表格、状态胶囊）。
 */
using WinDisplay.Ui;

namespace WinDisplay
{
    partial class Form_net
    {
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel body;
        private PageHeader header;
        private CardPanel cardDevice;
        private CardPanel cardList;
        private System.Windows.Forms.FlowLayoutPanel deviceFlow;
        private Label label1;
        private ComboBox combo_devList;
        private FlatButton but_start;
        private FlatButton but_stop;
        private FlatButton but_open_wf;
        private Label lblDeviceHint;
        private StatusPill pillState;
        private DataGridView dataGridView1;
        private Label lblGridHint;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dataGridView1?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            lblGridHint = new Label();
            root = new System.Windows.Forms.TableLayoutPanel();
            body = new System.Windows.Forms.TableLayoutPanel();
            cardDevice = new CardPanel();
            deviceFlow = new System.Windows.Forms.FlowLayoutPanel();
            label1 = new Label();
            combo_devList = new ComboBox();
            but_start = new FlatButton();
            but_stop = new FlatButton();
            but_open_wf = new FlatButton();
            lblDeviceHint = new Label();
            cardList = new CardPanel();
            pillState = new StatusPill();
            header = new PageHeader();
            root.SuspendLayout();
            body.SuspendLayout();
            cardDevice.SuspendLayout();
            deviceFlow.SuspendLayout();
            cardList.SuspendLayout();
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
            header.Description = "实时列出每个进程的连接与速率；需要先装 Npcap，并以管理员身份运行才能抓到包。";
            header.Dock = DockStyle.Fill;
            header.Name = "header";
            header.Title = "网络流量分析";
            // 
            // body
            // 
            body.ColumnCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.Controls.Add(cardDevice, 0, 0);
            body.Controls.Add(cardList, 0, 1);
            body.Dock = DockStyle.Fill;
            body.Name = "body";
            body.RowCount = 2;
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // 
            // cardDevice
            // 
            cardDevice.Controls.Add(deviceFlow);
            cardDevice.Dock = DockStyle.Fill;
            cardDevice.Margin = new Padding(0, 0, 0, UiTheme.GapM);
            cardDevice.Name = "cardDevice";
            cardDevice.Title = "抓取设置";
            // 
            // deviceFlow
            // 
            deviceFlow.AutoSize = true;
            deviceFlow.Controls.Add(label1);
            deviceFlow.Controls.Add(combo_devList);
            deviceFlow.Controls.Add(but_start);
            deviceFlow.Controls.Add(but_stop);
            deviceFlow.Controls.Add(lblDeviceHint);
            deviceFlow.Controls.Add(but_open_wf);
            deviceFlow.Dock = DockStyle.Top;
            deviceFlow.Name = "deviceFlow";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = UiTheme.Body;
            label1.ForeColor = UiTheme.TextSecondary;
            label1.Margin = new Padding(0, 7, UiTheme.GapS, 0);
            label1.Name = "label1";
            label1.Text = "网络接口：";
            // 
            // combo_devList
            // 
            combo_devList.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_devList.FlatStyle = FlatStyle.Flat;
            combo_devList.Font = UiTheme.Body;
            combo_devList.ItemHeight = 19;
            combo_devList.Margin = new Padding(0, 2, UiTheme.GapM, 0);
            combo_devList.Name = "combo_devList";
            combo_devList.Width = 320;
            // 
            // but_start
            // 
            but_start.Click += but_start_Click;
            but_start.Name = "but_start";
            but_start.Text = "开始抓取";
            but_start.Variant = ButtonVariant.Primary;
            // 
            // but_stop
            // 
            but_stop.Click += but_stop_Click;
            but_stop.Name = "but_stop";
            but_stop.Text = "停止";
            // 
            // lblDeviceHint
            // 
            lblDeviceHint.AutoSize = true;
            lblDeviceHint.Font = UiTheme.Small;
            lblDeviceHint.ForeColor = UiTheme.TextTertiary;
            lblDeviceHint.Margin = new Padding(0, 9, 0, 0);
            lblDeviceHint.Name = "lblDeviceHint";
            lblDeviceHint.Text = "速率每 1 秒刷新一次，进程名按 PID 现查";
            // 
            // but_open_wf
            // 
            but_open_wf.Click += but_open_wf_Click;
            but_open_wf.Name = "but_open_wf";
            but_open_wf.Text = "打开系统防火墙";
            but_open_wf.Variant = ButtonVariant.Ghost;
            // 
            // cardList
            // 
            cardList.Controls.Add(pillState);
            cardList.Controls.Add(dataGridView1);
            cardList.Controls.Add(lblGridHint);
            cardList.HeaderAction = pillState;
            cardList.Description = "按进程聚合的实时连接；启动后每秒刷新一次";
            cardList.Dock = DockStyle.Fill;
            cardList.Name = "cardList";
            cardList.Title = "连接列表";
            // 
            // pillState
            // 
            pillState.DotColor = UiTheme.TextTertiary;
            pillState.Name = "pillState";
            pillState.Text = "未启动";
            // 
            // dataGridView1
            // 
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Name = "dataGridView1";
            UiTheme.ApplyGrid(dataGridView1);
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.EnableHeadersVisualStyles = false;
            // 
            // lblGridHint
            // 
            lblGridHint.Dock = DockStyle.Fill;
            lblGridHint.Font = UiTheme.Body;
            lblGridHint.ForeColor = UiTheme.TextTertiary;
            lblGridHint.Name = "lblGridHint";
            lblGridHint.Text = "还没有开始抓取 —— 选一个网卡，点「开始抓取」。";
            lblGridHint.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form_net
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.WindowBackground;
            ClientSize = new Size(1184, 470);
            Controls.Add(root);
            Font = UiTheme.Body;
            MinimumSize = new Size(760, 420);
            Name = "Form_net";
            Text = "网络监控";
            FormClosing += Form_net_FormClosing;
            Load += Form_net_Load;
            root.ResumeLayout(false);
            body.ResumeLayout(false);
            cardDevice.ResumeLayout(false);
            deviceFlow.ResumeLayout(false);
            deviceFlow.PerformLayout();
            cardList.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
