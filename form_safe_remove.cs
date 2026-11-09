/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：form_safe_remove.cs
 * 作者：zhenhua
 * 说明：「文件粉碎」模块：选择文件后调用 util/SecureFileEraser 做不可恢复删除，
 * 进度与结果统一显示在卡片状态胶囊上（嵌入主窗体后没有标题栏，写窗体 Text 是看不见的）。
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
    /// 「文件粉碎」模块窗体：选一个文件 → 确认 → 交给 <see cref="SecureFileEraser"/> 用随机数据
    /// 覆写若干遍再删。覆写逻辑不在这里，窗体只负责参数收集、防重入、进度回显。
    /// 只支持单文件：多选、目录递归覆写没实现，就别在界面上给人选。
    /// </summary>
    public partial class form_safe_remove : Form
    {
        /// <summary>构造：铺完控件把状态胶囊置成「未选择文件」，执行按钮此时是禁用的。</summary>
        public form_safe_remove()
        {
            InitializeComponent();
            SetState("未选择文件", UiTheme.Border, UiTheme.SurfaceMuted, UiTheme.TextTertiary);
        }

        /// <summary>
        /// 卡片右上角那颗状态胶囊。模块被嵌进主窗体后没有标题栏，
        /// 进度和结果只能显示在界面里，写窗体 Text 是看不见的。
        /// </summary>
        private void SetState(string text, Color dot, Color back, Color fore)
        {
            pillState.SetState(text, dot, back, fore);
        }

        /// <summary>「选择文件」：单选 + <c>CheckFileExists</c>，选好了才放开执行按钮并把进度条归零。</summary>
        private void but_selFile_Click(object sender, EventArgs e)
        {
           

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "选择要安全删除的文件";
                openFileDialog.Multiselect = false;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = openFileDialog.FileName;
                    but_exec.Enabled = true;
                    progressEraser.Value = 0;
                    SetState("已选择，可以开始", UiTheme.Accent, UiTheme.AccentSoft, UiTheme.AccentText);
                }
            }

        }

        /// <summary>擦除进行中标志，重入靠它拦（见 <see cref="button1_Click"/>）。</summary>
        private bool erasing = false;

        /// <summary>
        /// 「开始安全删除」：覆写 <c>num_count</c> 遍，勾选了就最后再盖一遍零，然后删除文件。
        /// <para>
        /// 流程：校路径 → 二次确认（不可恢复，必须明写次数与是否盖零） → 上锁禁按钮 →
        /// <c>Task.Run</c> 跑覆写（在后台线程，UI 不卡）→ <c>Progress&lt;T&gt;</c> 把百分比送回 UI 线程刷
        /// 进度条与胶囊 → 成功就禁掉执行按钮（文件已没了），失败则放开允许直接重试。
        /// </para>
        /// </summary>
        private async void button1_Click(object sender, EventArgs e)
        {
            if (erasing) return;    // 防重入：擦除途中再点一次会用两个句柄抢同一个文件

            string path = (txtPath.Text ?? string.Empty).Trim();
            if (path.Length == 0 || !File.Exists(path))
            {
                MessageBox.Show("请先选择要安全删除的文件。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool fillZero = cb_fillZero.Checked;
            int count = (int)num_count.Value;

            var result = MessageBox.Show(
                $"确定要安全删除 {path} 吗？\n将用随机数据覆盖 {count} 次{(fillZero ? "，最后再覆盖一遍零" : "")}。此操作不可恢复！",
                "确认安全删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            erasing = true;
            but_exec.Enabled = false;
            progressEraser.Value = 0;
            SetState("正在擦除 0%", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
            Cursor = Cursors.WaitCursor;

            try
            {
                // 原来在 UI 线程上同步跑完整个覆写：大文件 × 几遍 = 窗口长时间「未响应」，
                // 而且中途没有任何反馈。现在放后台线程，用 Progress<T> 把百分比送回 UI 线程。
                // Progress<T>.Report 是显式接口实现，必须按 IProgress<T> 调
                IProgress<(long done, long total)> progress = new Progress<(long done, long total)>(p =>
                {
                    if (p.total <= 0) return;
                    int percent = (int)Math.Min(100, p.done * 100 / p.total);
                    progressEraser.Value = percent;
                    SetState($"正在擦除 {percent}%", UiTheme.Warning, UiTheme.WarningSoft, UiTheme.WarningText);
                });

                await Task.Run(() => SecureFileEraser.OverwriteAndDelete(
                    path, count, 1024 * 1024, fillZero,
                    (done, total) => progress.Report((done, total))));

                progressEraser.Value = 100;
                SetState("已完成，文件已不可恢复", UiTheme.Success, UiTheme.SuccessSoft, UiTheme.SuccessText);
                MessageBox.Show("操作完成", "安全删除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                but_exec.Enabled = false;   // 成功后要重新选文件才能再操作
            }
            catch (Exception ex)
            {
                SetState("失败，可以重试", UiTheme.Danger, UiTheme.DangerSoft, UiTheme.DangerText);
                Program.WriteLog("安全删除失败：" + path, ex);
                MessageBox.Show("安全删除失败：\n" + ex.Message + "\n\n详细堆栈见日志：\n" + Program.LogFilePath,
                    "安全删除", MessageBoxButtons.OK, MessageBoxIcon.Error);
                but_exec.Enabled = true;    // 失败允许直接重试
            }
            finally
            {
                Cursor = Cursors.Default;
                erasing = false;
            }
        }
    }
}
