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

namespace WinDisplay
{
    public partial class form_win_safe : Form
    {
        public form_win_safe()
        {
            InitializeComponent();
        }

        private void but_evn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("eventvwr.exe");
                // 或者使用：Process.Start("eventvwr.msc");
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开事件查看器：" + ex.Message);
            }
        }

        private void but_res_mon_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("resmon.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开资源监视器：" + ex.Message);
            }
        }

        private void but_task_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("taskmgr.exe");

        }

        private void but_user_Click(object sender, EventArgs e)
        {
            try
            {
                //Process.Start("lusrmgr.msc");
                OpenLocalUsersAndGroups();
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开本地用户和组管理器：" + ex.Message);
            }
        }

        private void but_computer_Click(object sender, EventArgs e)
        {
            try
            {
                OpenComputerManagement();
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法打开计算机管理：" + ex.Message);
            }
        }
        static void OpenComputerManagement()
        {
            string mscFile = "compmgmt.msc";
            string fullPath = Path.Combine(Environment.SystemDirectory, mscFile);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"文件不存在：{fullPath}");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true, // 必须为 true
                    Verb = "runas"          // 建议提权，避免功能受限
                };

                Process.Start(startInfo);
                Console.WriteLine("已启动计算机管理。");
            }
            catch (System.ComponentModel.Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                Console.WriteLine("用户取消了管理员权限请求。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动失败：{ex.Message}");
            }
        }
        static void OpenLocalUsersAndGroups()
        {
            string mscFile = "lusrmgr.msc";
            string fullPath = Path.Combine(Environment.SystemDirectory, mscFile);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("当前系统不支持本地用户和组管理器（例如：家庭版 Windows）。");
                Console.WriteLine("正在尝试打开现代设置界面...");
                OpenModernAccountsSettings();
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
                Console.WriteLine("已启动本地用户和组管理器。");
            }
            catch (System.ComponentModel.Win32Exception ex) when ((uint)ex.ErrorCode == 0x80004005)
            {
                Console.WriteLine("用户取消了管理员权限请求。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动失败：{ex.Message}");
            }
        }

        static void OpenModernAccountsSettings()
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:accounts") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法打开账户设置：{ex.Message}");
            }
        }
    }
}
