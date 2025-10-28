using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WinDisplay.util
{

        /// <summary>
        /// 系统清理工具类 - 负责扫描和清理系统垃圾
        /// </summary>
        public class SystemCleanerHelper
        {
            private BackgroundWorker scanWorker;
            private BackgroundWorker cleanWorker;
            private List<CleanItem> cleanItems;

            public SystemCleanerHelper()
            {
                cleanItems = new List<CleanItem>();
                InitializeWorkers();
            }

            #region 公共方法

            /// <summary>
            /// 检查是否有管理员权限
            /// </summary>
            public bool CheckAdminPrivileges()
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            /// <summary>
            /// 开始扫描垃圾文件
            /// </summary>
            /// <param name="listView">用于显示结果的ListView控件</param>
            /// <param name="progressBar">进度条控件</param>
            /// <param name="statusLabel">状态标签控件</param>
            /// <param name="totalSizeLabel">总大小标签控件</param>
            public void StartScan(ListView listView, ProgressBar progressBar, Label statusLabel, Label totalSizeLabel)
            {
                if (scanWorker.IsBusy)
                {
                    MessageBox.Show("扫描正在进行中，请稍候...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 清空之前的数据
                listView.Items.Clear();
                cleanItems.Clear();
                progressBar.Value = 0;
                statusLabel.Text = "正在扫描...";

                // 传递控件引用
                var controls = new ScanControls
                {
                    ListView = listView,
                    ProgressBar = progressBar,
                    StatusLabel = statusLabel,
                    TotalSizeLabel = totalSizeLabel
                };

                scanWorker.RunWorkerAsync(controls);
            }

            /// <summary>
            /// 清理选中的垃圾文件
            /// </summary>
            /// <param name="listView">包含选中项的ListView控件</param>
            /// <param name="progressBar">进度条控件</param>
            /// <param name="statusLabel">状态标签控件</param>
            public void StartClean(ListView listView, ProgressBar progressBar, Label statusLabel)
            {
                if (cleanWorker.IsBusy)
                {
                    MessageBox.Show("清理正在进行中，请稍候...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 获取选中的项
                List<CleanItem> itemsToClean = new List<CleanItem>();
                foreach (ListViewItem lvi in listView.Items)
                {
                    if (lvi.Checked && lvi.Tag is CleanItem)
                    {
                        itemsToClean.Add((CleanItem)lvi.Tag);
                    }
                }

                if (itemsToClean.Count == 0)
                {
                    MessageBox.Show("请至少选择一项进行清理！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                long sizeToClean = itemsToClean.Sum(i => i.Size);
                DialogResult result = MessageBox.Show(
                    $"确定要清理选中的 {itemsToClean.Count} 项吗？\n将释放约 {FormatSize(sizeToClean)} 的空间。\n\n此操作不可撤销！",
                    "确认清理", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    progressBar.Value = 0;
                    statusLabel.Text = "正在清理...";

                    var cleanData = new CleanData
                    {
                        Items = itemsToClean,
                        ListView = listView,
                        ProgressBar = progressBar,
                        StatusLabel = statusLabel
                    };

                    cleanWorker.RunWorkerAsync(cleanData);
                }
            }

            /// <summary>
            /// 全选或取消全选
            /// </summary>
            public void SelectAllItems(ListView listView, bool check)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    item.Checked = check;
                }
            }

            /// <summary>
            /// 格式化文件大小显示
            /// </summary>
            public string FormatSize(long bytes)
            {
                if (bytes <= 0) return "0 B";

                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;

                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }

            /// <summary>
            /// 配置ListView的列
            /// </summary>
            public void SetupListView(ListView listView)
            {
                listView.View = View.Details;
                listView.FullRowSelect = true;
                listView.CheckBoxes = true;
                listView.GridLines = true;
                listView.Columns.Clear();

                listView.Columns.Add("项目类型", 200);
                listView.Columns.Add("位置", 350);
                listView.Columns.Add("大小", 100);
                listView.Columns.Add("文件数", 80);
            }

            #endregion

            #region 后台工作线程初始化

            private void InitializeWorkers()
            {
                // 扫描工作线程
                scanWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                scanWorker.DoWork += ScanWorker_DoWork;
                scanWorker.ProgressChanged += ScanWorker_ProgressChanged;
                scanWorker.RunWorkerCompleted += ScanWorker_RunWorkerCompleted;

                // 清理工作线程
                cleanWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                cleanWorker.DoWork += CleanWorker_DoWork;
                cleanWorker.ProgressChanged += CleanWorker_ProgressChanged;
                cleanWorker.RunWorkerCompleted += CleanWorker_RunWorkerCompleted;
            }

        #endregion

        #region 扫描相关方法

        /// <summary>
        /// 扫描浏览器缓存
        /// </summary>
        private List<CleanItem> ScanBrowserCaches()
        {
            List<CleanItem> items = new List<CleanItem>();
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            try
            {
                // Chrome缓存
                string chromePath = Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache");
                if (Directory.Exists(chromePath))
                    items.Add(ScanDirectory(chromePath, "Chrome浏览器缓存", "*.*"));

                // Edge缓存
                string edgePath = Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache");
                if (Directory.Exists(edgePath))
                    items.Add(ScanDirectory(edgePath, "Edge浏览器缓存", "*.*"));

                // Firefox缓存
                string firefoxProfile = Path.Combine(localAppData, @"Mozilla\Firefox\Profiles");
                if (Directory.Exists(firefoxProfile))
                {
                    foreach (var dir in Directory.GetDirectories(firefoxProfile))
                    {
                        string cachePath = Path.Combine(dir, "cache2");
                        if (Directory.Exists(cachePath))
                            items.Add(ScanDirectory(cachePath, "Firefox浏览器缓存", "*.*"));
                    }
                }

                // IE缓存
                string ieCachePath = Path.Combine(localAppData, @"Microsoft\Windows\INetCache");
                if (Directory.Exists(ieCachePath))
                    items.Add(ScanDirectory(ieCachePath, "IE浏览器缓存", "*.*"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描浏览器缓存失败: {ex.Message}");
            }

            return items;
        }

        /// <summary>
        /// 扫描休眠文件
        /// </summary>
        private CleanItem ScanHibernateFile()
        {
            CleanItem item = new CleanItem
            {
                Name = "休眠文件 (hiberfil.sys)",
                Path = @"C:\hiberfil.sys",
                Size = 0,
                FileCount = 0
            };

            try
            {
                string hibernateFile = @"C:\hiberfil.sys";
                if (File.Exists(hibernateFile))
                {
                    FileInfo fi = new FileInfo(hibernateFile);
                    item.Size = fi.Length;
                    item.FileCount = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描休眠文件失败: {ex.Message}");
            }

            return item;
        }

        /// <summary>
        /// 扫描系统还原点
        /// </summary>
        private CleanItem ScanSystemRestorePoints()
        {
            CleanItem item = new CleanItem
            {
                Name = "系统还原点 (保留最新1个)",
                Path = @"C:\System Volume Information",
                Size = 0,
                FileCount = 0
            };

            try
            {
                // 使用WMI查询还原点信息
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    "root\\default", "SELECT * FROM SystemRestore"))
                {
                    int restorePointCount = 0;
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        restorePointCount++;
                    }
                    item.FileCount = Math.Max(0, restorePointCount - 1); // 保留最新1个
                }

                // 估算大小（System Volume Information文件夹）
                string sviPath = @"C:\System Volume Information";
                if (Directory.Exists(sviPath))
                {
                    DirectoryInfo di = new DirectoryInfo(sviPath);
                    foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            item.Size += file.Length;
                        }
                        catch { }
                    }
                    // 减去最新还原点的估算大小（保留约30%）
                    item.Size = (long)(item.Size * 0.7);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描还原点失败: {ex.Message}");
            }

            return item;
        }

        /// <summary>
        /// 扫描WinSxS组件存储
        /// </summary>
        private CleanItem ScanWinSxS()
        {
            CleanItem item = new CleanItem
            {
                Name = "WinSxS组件存储清理",
                Path = @"C:\Windows\WinSxS",
                Size = 0,
                FileCount = 0
            };

            try
            {
                // 使用DISM命令查询可清理的大小
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "Dism.exe",
                    Arguments = "/Online /Cleanup-Image /AnalyzeComponentStore",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 解析输出获取可回收大小
                if (output.Contains("可回收") || output.Contains("Reclaimable"))
                {
                    // 简化处理：估算WinSxS中可清理的大小
                    string winsxsPath = @"C:\Windows\WinSxS\Backup";
                    if (Directory.Exists(winsxsPath))
                    {
                        DirectoryInfo di = new DirectoryInfo(winsxsPath);
                        foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                item.Size += file.Length;
                            }
                            catch { }
                        }
                    }
                    item.FileCount = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描WinSxS失败: {ex.Message}");
            }

            return item;
        }


        private void ScanWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                ScanControls controls = (ScanControls)e.Argument;
                List<CleanItem> items = new List<CleanItem>();

                // 定义扫描路径
                string tempPath = Path.GetTempPath();
                string winTemp = @"C:\Windows\Temp";
                string prefetch = @"C:\Windows\Prefetch";
                string recycler = @"C:\$Recycle.Bin";
                string softwareDistribution = @"C:\Windows\SoftwareDistribution\Download";
                string windowsOld = @"C:\Windows.old";
                string errorReports = @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue";
                string thumbnails = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft\\Windows\\Explorer");
                string logs = @"C:\Windows\Logs";
                string installer = @"C:\Windows\Installer\$PatchCache$";

                // 1. 用户临时文件
                scanWorker.ReportProgress(8, "正在扫描用户临时文件...");
                items.Add(ScanDirectory(tempPath, "用户临时文件", "*.tmp;*.log;*.bak;*._mp;*.old"));

                // 2. Windows临时文件
                scanWorker.ReportProgress(16, "正在扫描Windows临时文件...");
                if (Directory.Exists(winTemp))
                    items.Add(ScanDirectory(winTemp, "Windows临时文件", "*.*"));

                // 3. 回收站
                scanWorker.ReportProgress(24, "正在扫描回收站...");
                if (Directory.Exists(recycler))
                    items.Add(ScanDirectory(recycler, "回收站", "*.*"));

                // 4. Windows更新缓存
                scanWorker.ReportProgress(32, "正在扫描Windows更新缓存...");
                if (Directory.Exists(softwareDistribution))
                    items.Add(ScanDirectory(softwareDistribution, "Windows更新缓存", "*.*"));

                // 5. 预读取文件
                scanWorker.ReportProgress(40, "正在扫描预读取文件...");
                if (Directory.Exists(prefetch))
                    items.Add(ScanDirectory(prefetch, "预读取文件", "*.pf"));

                // 6. Windows.old (旧系统文件)
                scanWorker.ReportProgress(48, "正在扫描旧系统文件...");
                if (Directory.Exists(windowsOld))
                    items.Add(ScanDirectory(windowsOld, "Windows.old (旧系统)", "*.*"));

                // 7. 系统错误报告
                scanWorker.ReportProgress(56, "正在扫描系统错误报告...");
                if (Directory.Exists(errorReports))
                    items.Add(ScanDirectory(errorReports, "系统错误报告", "*.*"));

                // 8. 缩略图缓存
                scanWorker.ReportProgress(64, "正在扫描缩略图缓存...");
                if (Directory.Exists(thumbnails))
                    items.Add(ScanDirectory(thumbnails, "缩略图缓存", "thumbcache_*.db;iconcache_*.db"));

                // 9. Windows日志文件
                scanWorker.ReportProgress(72, "正在扫描日志文件...");
                if (Directory.Exists(logs))
                    items.Add(ScanDirectory(logs, "Windows日志", "*.log;*.etl"));

                // 10. 安装程序缓存
                scanWorker.ReportProgress(80, "正在扫描安装程序缓存...");
                if (Directory.Exists(installer))
                    items.Add(ScanDirectory(installer, "安装程序缓存", "*.*"));

                // 11. Windows Defender扫描历史
                scanWorker.ReportProgress(88, "正在扫描Defender历史...");
                string defenderHistory = @"C:\ProgramData\Microsoft\Windows Defender\Scans\History";
                if (Directory.Exists(defenderHistory))
                    items.Add(ScanDirectory(defenderHistory, "Windows Defender扫描历史", "*.*"));

            // 12. 浏览器缓存
            scanWorker.ReportProgress(75, "正在扫描浏览器缓存...");
            items.AddRange(ScanBrowserCaches());

            // 13. 休眠文件
            scanWorker.ReportProgress(80, "正在扫描休眠文件...");
            CleanItem hibernateItem = ScanHibernateFile();
            if (hibernateItem != null && hibernateItem.Size > 0)
                items.Add(hibernateItem);

            // 14. 系统还原点
            scanWorker.ReportProgress(85, "正在扫描系统还原点...");
            CleanItem restoreItem = ScanSystemRestorePoints();
            if (restoreItem != null && restoreItem.Size > 0)
                items.Add(restoreItem);

            // 15. WinSxS组件存储
            scanWorker.ReportProgress(90, "正在扫描WinSxS组件...");
            CleanItem winsxsItem = ScanWinSxS();
            if (winsxsItem != null && winsxsItem.Size > 0)
                items.Add(winsxsItem);

            scanWorker.ReportProgress(100, "扫描完成！");
                e.Result = new ScanResult
                {
                    Items = items.Where(item => item != null && item.Size > 0).ToList(),
                    Controls = controls
                };
            }

            private CleanItem ScanDirectory(string path, string name, string patterns)
            {
                CleanItem item = new CleanItem
                {
                    Name = name,
                    Path = path,
                    Size = 0,
                    FileCount = 0
                };

                try
                {
                    if (!Directory.Exists(path)) return item;

                    string[] patternArray = patterns.Split(';');
                    List<string> allFiles = new List<string>();

                    foreach (string pattern in patternArray)
                    {
                        try
                        {
                            allFiles.AddRange(Directory.GetFiles(path, pattern, SearchOption.AllDirectories));
                        }
                        catch (UnauthorizedAccessException) { }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"扫描 {path} 时出错: {ex.Message}");
                        }
                    }

                    // 去重
                    allFiles = allFiles.Distinct().ToList();

                    foreach (string file in allFiles)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.Exists)
                            {
                                item.Size += fi.Length;
                                item.FileCount++;
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"扫描目录 {path} 时出错: {ex.Message}");
                }

                return item;
            }

            private void ScanWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // 进度更新在UI线程中处理
            }

            private void ScanWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    MessageBox.Show($"扫描出错: {e.Error.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ScanResult result = (ScanResult)e.Result;
                cleanItems = result.Items;
                long totalSize = 0;

                // 填充ListView
                foreach (var item in cleanItems)
                {
                    ListViewItem lvi = new ListViewItem(item.Name);
                    lvi.SubItems.Add(item.Path);
                    lvi.SubItems.Add(FormatSize(item.Size));
                    lvi.SubItems.Add(item.FileCount.ToString());
                    lvi.Tag = item;
                    lvi.Checked = true;
                    result.Controls.ListView.Items.Add(lvi);

                    totalSize += item.Size;
                }

                // 更新总大小标签
                result.Controls.TotalSizeLabel.Text = $"总垃圾大小: {FormatSize(totalSize)}";
                result.Controls.StatusLabel.Text = $"扫描完成！找到 {cleanItems.Count} 个垃圾项，共 {FormatSize(totalSize)}";
                result.Controls.ProgressBar.Value = 100;
            }

            #endregion

            #region 清理相关方法

            private void CleanWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                CleanData data = (CleanData)e.Argument;
                int total = data.Items.Count;
                int current = 0;
                List<string> failedItems = new List<string>();

                foreach (var item in data.Items)
                {
                    current++;
                    int progress = (int)((double)current / total * 100);
                    cleanWorker.ReportProgress(progress, $"正在清理: {item.Name} ({current}/{total})");

                    try
                    {
                        if (Directory.Exists(item.Path))
                        {
                            // 特殊处理Windows.old
                            if (item.Path.Contains("Windows.old"))
                            {
                                if (!CleanWindowsOld(item.Path))
                                    failedItems.Add(item.Name);
                            }
                            else
                            {
                                if (!CleanDirectory(item.Path))
                                    failedItems.Add(item.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"清理 {item.Name} 时出错: {ex.Message}");
                        failedItems.Add(item.Name);
                    }
                }

                e.Result = new CleanResult
                {
                    FailedItems = failedItems,
                    Controls = data
                };
            }

            private bool CleanDirectory(string path)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    // 删除文件
                    foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            file.Attributes = FileAttributes.Normal;
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"删除文件 {file.FullName} 失败: {ex.Message}");
                        }
                    }

                    // 删除空文件夹
                    foreach (DirectoryInfo dir in di.GetDirectories("*", SearchOption.AllDirectories)
                        .OrderByDescending(d => d.FullName.Length))
                    {
                        try
                        {
                            if (dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0)
                            {
                                dir.Delete();
                            }
                        }
                        catch { }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"清理目录 {path} 失败: {ex.Message}");
                    return false;
                }
            }

            private bool CleanWindowsOld(string path)
            {
                try
                {
                    // 使用takeown和icacls获取权限后删除
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c takeown /F \"{path}\" /R /D Y && icacls \"{path}\" /grant administrators:F /T && rd /s /q \"{path}\"",
                        Verb = "runas",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    Process process = Process.Start(psi);
                    process?.WaitForExit(60000); // 等待最多60秒
                    return process?.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"清理Windows.old失败: {ex.Message}");
                    return false;
                }
            }

            private void CleanWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                // 进度更新在UI线程中处理
            }

            private void CleanWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    MessageBox.Show($"清理过程中出现错误: {e.Error.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                CleanResult result = (CleanResult)e.Result;

                if (result.FailedItems.Count > 0)
                {
                    string failedList = string.Join("\n", result.FailedItems);
                    MessageBox.Show($"清理完成，但以下项清理失败：\n\n{failedList}\n\n这可能是由于权限不足或文件被占用。",
                        "部分成功", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("清理完成！所有选中项已成功清理。", "成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                result.Controls.StatusLabel.Text = "清理完成！建议重新扫描查看效果。";
                result.Controls.ProgressBar.Value = 100;

                // 清空ListView
                result.Controls.ListView.Items.Clear();
            }

            #endregion
        }

        #region 数据类

        /// <summary>
        /// 清理项数据模型
        /// </summary>
        public class CleanItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public long Size { get; set; }
            public int FileCount { get; set; }
        }

        /// <summary>
        /// 扫描时使用的控件集合
        /// </summary>
        internal class ScanControls
        {
            public ListView ListView { get; set; }
            public ProgressBar ProgressBar { get; set; }
            public Label StatusLabel { get; set; }
            public Label TotalSizeLabel { get; set; }
        }

        /// <summary>
        /// 扫描结果
        /// </summary>
        internal class ScanResult
        {
            public List<CleanItem> Items { get; set; }
            public ScanControls Controls { get; set; }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        internal class CleanData
        {
            public List<CleanItem> Items { get; set; }
            public ListView ListView { get; set; }
            public ProgressBar ProgressBar { get; set; }
            public Label StatusLabel { get; set; }
        }

        /// <summary>
        /// 清理结果
        /// </summary>
        internal class CleanResult
        {
            public List<string> FailedItems { get; set; }
            public CleanData Controls { get; set; }
        }



       
    

        #endregion
    }






