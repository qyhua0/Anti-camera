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
    public partial class form_safe_remove : Form
    {
        public form_safe_remove()
        {
            InitializeComponent();
        }

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
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Boolean fillZero = cb_fillZero.Checked;
            int count = ((int)num_count.Value);

            String path = txtPath.Text;
            var result = MessageBox.Show(
           $"确定要安全删除此{path}吗？此操作不可恢复！\n{txtPath.Text}",
           "确认安全删除",
           MessageBoxButtons.YesNo,
           MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;


            SecureFileEraser.OverwriteAndDelete(path, count, 1024 * 1024,fillZero,null);

            MessageBox.Show("操作完成");
            but_exec.Enabled=false;
        }
    }
}
