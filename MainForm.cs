using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDisplay
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitTabs();
            // Dock 到父容器，随窗体自适应
            tabControl1.Dock = DockStyle.Fill;

            // Tab 外观设置
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.ItemSize = new Size(120, 30);
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            // 绑定绘制事件
            tabControl1.DrawItem += TabControl1_DrawItem;
        }

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var tabPage = tabControl1.TabPages[e.Index];
            var tabRect = e.Bounds;

            // 是否是选中的 Tab
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // 背景色
            Color backColor = isSelected ? Color.DodgerBlue : Color.LightGray;
            using (Brush brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, tabRect);
            }

            // 文字
            TextRenderer.DrawText(
                g,
                tabPage.Text,
                this.Font,
                tabRect,
                isSelected ? Color.White : Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void InitTabs()
        {
            tabControl1.TabPages.Clear();
            this.Dock = DockStyle.Fill;
            // 防监控模块
            var screenGuard = new frm_screen();
            LoadFormInTab(screenGuard, "屏幕防拍摄");

            // 网络流量分析模块
            var netAnalyzer = new Form_net();
            LoadFormInTab(netAnalyzer, "网络流量安全分析");

            // 安全控制面板模块
            var securePanel = new form_win_safe();
            LoadFormInTab(securePanel, "安全功能面板");

            // 清理垃圾模块
            var formClear = new form_clear();
            LoadFormInTab(formClear, "清理垃圾模块");
        }

        /// <summary>
        /// 把 Form 加载到 TabPage
        /// </summary>
        private void LoadFormInTab(Form form, string tabTitle)
        {
            var tabPage = new TabPage(tabTitle);

            form.TopLevel = false;                         // 取消顶级窗口
            form.FormBorderStyle = FormBorderStyle.None;   // 去掉边框
            form.Dock = DockStyle.Fill;                    // 填充整个 TabPage

            tabPage.Controls.Add(form);
            tabControl1.TabPages.Add(tabPage);

            form.Show();  // 必须调用，否则不显示
        }
    }
}
