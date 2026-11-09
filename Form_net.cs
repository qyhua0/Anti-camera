/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：Form_net.cs
 * 作者：zhenhua
 * 说明：「网络流量分析」模块：列出网卡、启停 Npcap 抓包、按进程显示实时连接与带宽，
 * 抓取状态显示在卡片右上角胶囊里（模块嵌在主窗体内，只弹 MessageBox 等于没有状态）。
 */
using SharpPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDisplay.Ui;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace WinDisplay
{
    /// <summary>
    /// 「网络流量分析」模块窗体：选网卡 → 交给 <see cref="NpcapNetworkMonitor"/> 抓包，
    /// 把每条连接/监听端口按进程列在表格里。抓包本身全在 monitor 里，本窗体只管网卡选择、
    /// 启停按钮互锁、状态胶囊。
    /// 需要 Npcap 驱动 + 管理员权限：没装驱动时 <see cref="LoadNetworkDevices"/> 就拿到空列表，
    /// 界面会直接写「请检查 Npcap 是否安装」。
    /// </summary>
    public partial class Form_net : Form
    {
        /// <summary>构造：只铺控件。设备列表要等 Load（那时 Npcap 才能枚举到设备）。</summary>
        public Form_net()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 载入：枚举网卡、把「停止」先禁掉、根据能不能选到设备定状态胶囊。
        /// 这里用 <c>combo_devList.Enabled</c> 当「有可用网卡」的标志（<see cref="LoadNetworkDevices"/>
        /// 枚举不到设备时会把它置灰），不另存一份布尔量。
        /// </summary>
        private void Form_net_Load(object sender, EventArgs e)
        {
            //  NpcapNetworkMonitor.Start(dataGridView1);
            LoadNetworkDevices();
            but_stop.Enabled = false;
            if (combo_devList.Enabled)
            {
                SetState("已就绪，可以开始", UiTheme.Accent, UiTheme.AccentSoft, UiTheme.AccentText);
            }
            else
            {
                SetState("没有可用网卡：请检查 Npcap 是否安装", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
            }
        }

        /// <summary>
        /// 抓取状态写成卡片右上角的胶囊：模块嵌在主窗体里，只靠弹窗提醒等于没有状态。
        /// </summary>
        private void SetState(string text, Color dot, Color back, Color fore)
        {
            pillState.SetState(text, dot, back, fore);
            // 空列表得说清楚为什么空；但停止之后表格里还留着最后一轮数据，别把它盖住
            lblGridHint.Visible = !running && dataGridView1.Rows.Count == 0;
        }

        /// <summary>当前是否在抓取。除了控按钮使能，还决定空列表提示要不要显示（见 <see cref="SetState"/>）。</summary>
        private bool running;

        /// <summary>
        /// 用 SharpPcap 的 <see cref="CaptureDeviceList"/> 枚举网卡，填进下拉框（显示成
        /// <c>[序号] Description (Name)</c>），默认选第一项。序号直接当作 <see cref="NpcapNetworkMonitor.Start"/>
        /// 的 deviceIndex，所以这里不能排序、不能过滤，两边必须用同一个枚举顺序。
        /// </summary>
        private void LoadNetworkDevices()
        {
            try
            {
                var devices = CaptureDeviceList.Instance;

                if (devices == null || devices.Count == 0)
                {
                    combo_devList.Items.Add("未检测到任何网卡设备");
                    combo_devList.Enabled = false;
                    return;
                }

                // 清空现有项
                combo_devList.Items.Clear();

                // 遍历所有设备，添加到 ComboBox
                for (int i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    // 使用 Description 作为显示名称，可以包含更详细的信息
                    string displayName = $"[{i}] {device.Description}";
                    if (!string.IsNullOrEmpty(device.Name))
                    {
                        displayName += $" ({device.Name})";
                    }
                    combo_devList.Items.Add(displayName);
                }

                // 默认选择第一项
                if (combo_devList.Items.Count > 0)
                {
                    combo_devList.SelectedIndex = 0;
                }

                // 确保 ComboBox 可用
                combo_devList.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载网卡设备失败: {ex.Message}\n请确保已安装 Npcap 并以管理员权限运行此程序。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                combo_devList.Items.Add("加载设备列表出错");
                combo_devList.Enabled = false;
            }
        }


        /// <summary>
        /// 关窗即停：抓包线程是全局单例，窗体都关了还在跑就是泄漏 + 后台继续写已释放的表格。
        /// </summary>
        private void Form_net_FormClosing(object sender, FormClosingEventArgs e)
        {
            NpcapNetworkMonitor.Stop();
        }

        /// <summary>Designer 里遗留的第二个停止入口（与 <see cref="but_stop_Click"/> 同一件事），保留只为不断 Designer 的事件连线。</summary>
        private void button1_Click(object sender, EventArgs e)
        {

            NpcapNetworkMonitor.Stop();

        }

        /// <summary>
        /// 「开始抓取」：用下拉框选中的网卡起 monitor，每秒刷新一次表格；
        /// 起来后开始/停止按钮互锁，防重入（monitor 是全局单例，开两次会互相盖掉）。
        /// </summary>
        private void but_start_Click(object sender, EventArgs e)
        {
            // 检查是否有设备可选
            if (combo_devList.Items.Count == 0 || combo_devList.SelectedIndex < 0)
            {
                MessageBox.Show("请先选择一个网卡设备。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取用户选择的设备索引
            int selectedDeviceIndex = combo_devList.SelectedIndex;

            try
            {
                // 调用 NpcapNetworkMonitor.Start，传入选择的设备索引
                NpcapNetworkMonitor.Start(dataGridView1, deviceIndex: selectedDeviceIndex, refreshMs: 1000);
                but_start.Enabled = false;
                but_stop.Enabled = true;
                running = true;
                SetState("正在抓取 · 每秒刷新", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }
            catch (Exception ex)
            {
                Program.WriteLog("启动网络监控失败", ex);
                SetState("启动失败，可重试", UiTheme.Danger, UiTheme.DangerSoft, UiTheme.DangerText);
                MessageBox.Show($"启动监控失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>「停止抓取」：停线程 + 按钮恢复 + 状态胶囊改「已停止」。表格里最后一轮数据留着给人看。</summary>
        private void but_stop_Click(object sender, EventArgs e)
        {
            NpcapNetworkMonitor.Stop();
            but_start.Enabled = true;
            but_stop.Enabled = false;
            running = false;
            SetState("已停止", UiTheme.TextTertiary, UiTheme.SurfaceMuted, UiTheme.TextSecondary);
        }

        /// <summary>「系统防火墙」按钮：可定 wf.msc 看入站规则，排查「这个进程能不能联网」。</summary>
        private void but_open_wf_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFirewall();
            }
            catch (Exception ex)
            {
                // 打不开也要在界面上说清楚，不能什么都不显示
                Program.WriteLog("打开系统防火墙失败", ex);
                MessageBox.Show($"没能打开系统防火墙：{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 打开 wf.msc。走 <c>cmd /c start</c> 而不是直接 Process.Start("wf.msc")：
        /// .msc 是文档类型，必须交给 Shell；CreateNoWindow + Hidden 是为了不闪一个黑框。
        /// </summary>
        private static void OpenFirewall()
        {
            // Process 对象拿着内核句柄，不 Dispose 就是每点一次漏一个
            using Process? wf = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c start wf.msc",
                CreateNoWindow = true,
                UseShellExecute = true,  // 必须为 true 才能使用 "start"
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}
