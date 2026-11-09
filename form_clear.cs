/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_clear.cs
 * 作者：zhenhua
 * 说明：「垃圾清理」模块的界面壳子：扫描/停止/清理三个动作 + 结果列表，
 * 真正的扫描与删除策略全在 util/SystemCleanerHelper 里，这里只做线程调度和界面回显。
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDisplay.Ui;
using WinDisplay.util;

namespace WinDisplay
{
    /// <summary>
    /// 「垃圾清理」模块窗体：只当界面壳子——把 <see cref="SystemCleanerHelper"/> 接到
    /// 扫描/清理两个按钮上，再把它的进度、状态、列表回显接上。
    /// 什公算垃圾、哪些路径能动、删不掉怎么处理，全在 helper 里，这里不重复定策略。
    /// </summary>
    public partial class form_clear : Form
    {
        /// <summary>
        /// 清理引擎。一个窗体一个实例：它内部自己持扫描/删除用的 BackgroundWorker，
        /// 不在多个窗体间共享，否则两个窗体会互相抢同一个 worker。
        /// </summary>
        private SystemCleanerHelper cleanerHelper;

        /// <summary>
        /// 构造：建 helper → 把列表交给它建列 → 绑四个按钮。
        /// 权限差异在这里只写状态胶囊不弹框（模块是嵌在主窗体里的子窗体，一打开就弹模态框会盖住整界面）。
        /// </summary>
        public form_clear()
        {
            InitializeComponent();

            // 初始化清理工具
            cleanerHelper = new SystemCleanerHelper();

            // 配置ListView（只需调用一次）
            cleanerHelper.SetupListView(listViewResults);
            UiTheme.ApplyListView(listViewResults);
            UiTheme.ShowListEmptyHint(listViewResults, "还没有扫描结果 —— 点「扫描垃圾文件」开始。");

            // 权限提示改成卡片上的状态胶囊：模块嵌在主窗体里，一打开就弹模态框很突兀，
            // 切到别的标签页也会被它挡住。
            if (cleanerHelper.CheckAdminPrivileges())
            {
                pillPermission.SetState("管理员权限", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
            }
            else
            {
                pillPermission.SetState("普通权限 · Windows.old 等项清理不了", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
            }

            // 绑定按钮事件
            btnScan.Click += BtnScan_Click;
            btnClean.Click += BtnClean_Click;
            btnSelectAll.Click += (s, e) => cleanerHelper.SelectAllItems(listViewResults, true);
            btnDeselectAll.Click += (s, e) => cleanerHelper.SelectAllItems(listViewResults, false);

        }

        /// <summary>Designer 里还绑着的 Load 入口，现在什么都没做；初始化全部移到构造函数，因为嵌入主窗体时 Load 的时机比控件就绪晚。</summary>
        private void form_clear_Load(object sender, EventArgs e)
        {

        }

        /// <summary>「扫描垃圾文件」：异步扫描，进度回 progressBar1 / lblStatus，扫完把可释放总量写 lblTotalSize。</summary>
        private void BtnScan_Click(object sender, EventArgs e)
        {
            // 传入您的控件即可
            cleanerHelper.StartScan(listViewResults, progressBar1, lblStatus, lblTotalSize);
        }

        /// <summary>「清理选中项」：只删列表里勾选的行；二次确认与删完后的刷新都在 helper 里。</summary>
        private void BtnClean_Click(object sender, EventArgs e)
        {
            // 传入您的控件即可
            cleanerHelper.StartClean(listViewResults, progressBar1, lblStatus);
        }

    }
}
