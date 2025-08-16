using SharpPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace WinDisplay
{
    public partial class Form_net : Form
    {
        public Form_net()
        {
            InitializeComponent();
        }

        private void Form_net_Load(object sender, EventArgs e)
        {
            //  NpcapNetworkMonitor.Start(dataGridView1);
            LoadNetworkDevices();


        }

        /// <summary>
        /// 加载所有可用的网络设备到 ComboBox
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


        private void Form_net_FormClosing(object sender, FormClosingEventArgs e)
        {
            NpcapNetworkMonitor.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            NpcapNetworkMonitor.Stop();

        }

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
                // btnStart.Enabled = false;
                // btnStop.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动监控失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
