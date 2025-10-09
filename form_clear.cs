using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDisplay.util;

namespace WinDisplay
{
    public partial class form_clear : Form
    {
        private SystemCleanerHelper cleanerHelper;



        public form_clear()
        {
            InitializeComponent();

            // 初始化清理工具
            cleanerHelper = new SystemCleanerHelper();

            // 配置ListView（只需调用一次）
            cleanerHelper.SetupListView(listViewResults);

            // 检查管理员权限
            if (!cleanerHelper.CheckAdminPrivileges())
            {
                MessageBox.Show("建议以管理员身份运行以获得完整功能！",
                    "权限提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 绑定按钮事件
            btnScan.Click += BtnScan_Click;
            btnClean.Click += BtnClean_Click;
            btnSelectAll.Click += (s, e) => cleanerHelper.SelectAllItems(listViewResults, true);
            btnDeselectAll.Click += (s, e) => cleanerHelper.SelectAllItems(listViewResults, false);

        }

        private void form_clear_Load(object sender, EventArgs e)
        {

        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            // 传入您的控件即可
            cleanerHelper.StartScan(listViewResults, progressBar1, lblStatus, lblTotalSize);
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            // 传入您的控件即可
            cleanerHelper.StartClean(listViewResults, progressBar1, lblStatus);
        }

        private void lblTotalSize_Click(object sender, EventArgs e)
        {

        }
    }
}
