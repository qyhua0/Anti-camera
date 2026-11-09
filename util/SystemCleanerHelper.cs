/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：util/SystemCleanerHelper.cs
 * 作者：zhenhua
 * 说明：系统垃圾扫描与清理：临时目录、浏览器/系统缓存、更新残留、Windows.old 等，
 * 「扫到什么才算垃圾」的判断全部在这里，后台线程扫描 + 进度回报，删除前按类型二次确认。
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDisplay.Ui;

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

            /// <summary>本次扫描产生的项（引用相等）。清理时只接受这些实例，杜绝凭据不明/过期的列表项</summary>
            private readonly HashSet<CleanItem> _scanned = new HashSet<CleanItem>();

            /// <summary>本次扫描登记过的根目录。清理只允许发生在这些目录之内</summary>
            private readonly HashSet<string> _roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private const string HiberfilPath = @"C:\hiberfil.sys";
            private const string SystemVolumeInfoPath = @"C:\System Volume Information";
            private const string WinSxSPath = @"C:\Windows\WinSxS";

            /// <summary>Windows.old 位于系统盘，与扫描/清理共用同一个常量，避免两处写法不一致</summary>
            private static readonly string WindowsOldPath = Path.Combine(
                Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) ?? @"C:\",
                "Windows.old");

            /// <summary>构造时立刻建好两个 BackgroundWorker：扫描/清理都靠它们把活丢到线程池、回调用 SynchronizationContext 送回 UI 线程</summary>
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

                // 清空之前的数据（同时作废上一轮的清理白名单）
                listView.Items.Clear();
                cleanItems.Clear();
                _scanned.Clear();
                _roots.Clear();
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

                // 安全护栏：仅清理「本次扫描产生 + 位于登记范围内 + 允许删除类型」的项
                List<CleanItem> skipped = new List<CleanItem>();
                itemsToClean.RemoveAll(it =>
                {
                    if (it.Kind == CleanKind.InfoOnly || !_scanned.Contains(it) || !IsCleanablePath(it.Path))
                    {
                        skipped.Add(it);
                        return true;
                    }
                    return false;
                });

                if (skipped.Count > 0)
                {
                    MessageBox.Show(
                        $"以下 {skipped.Count} 项不能由本工具直接删除，已自动跳过：\n\n" +
                        string.Join("\n", skipped.Select(b => "· " + b.Name)) +
                        "\n\n请按各项名称中的提示手动处理。",
                        $"已跳过 {skipped.Count} 项", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (itemsToClean.Count == 0) return;

                // 清理前给出清单预览，让用户知道接下来要动的是哪些位置
                long sizeToClean = itemsToClean.Sum(i => i.Size);
                string preview = string.Join("\n", itemsToClean.Take(10).Select(i =>
                    $"  · {i.Name}：{FormatSize(i.Size)} / {i.FileCount} 个文件"));
                if (itemsToClean.Count > 10)
                    preview += $"\n  · … 以及另外 {itemsToClean.Count - 10} 项";

                DialogResult result = MessageBox.Show(
                    $"确定要清理选中的 {itemsToClean.Count} 项吗？\n\n{preview}\n\n合计约 {FormatSize(sizeToClean)}。\n" +
                    "只会删除上述位置中、符合该项扫描时列出的文件类型的文件，不会波及其它文件。\n\n此操作不可撤销！",
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
                    // 仅统计项（休眠文件/还原点/WinSxS/Installer 缓存）不参与全选
                    item.Checked = check && (!(item.Tag is CleanItem ci) || ci.Kind != CleanKind.InfoOnly);
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
            // hiberfil.sys 是受保护的系统文件，删除它等价于关闭休眠功能，必须由用户自己用 powercfg 决定
            CleanItem item = new CleanItem
            {
                Name = "休眠文件 (hiberfil.sys) — 请自行执行 powercfg /h off 关闭休眠",
                Path = HiberfilPath,
                Size = 0,
                FileCount = 0,
                Kind = CleanKind.InfoOnly
            };

            try
            {
                string hibernateFile = HiberfilPath;
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
            // 只读统计。删除还原点必须走 VSS / 「磁盘清理」，直接删 System Volume Information 里的文件
            // 会破坏卷影副本数据库，且这个目录还被日志、卷挂载点使用，绝不可能靠匹配文件名安全清理。
            CleanItem item = new CleanItem
            {
                Name = "系统还原点占用 — 请在「磁盘清理」或系统属性中手动清理",
                Path = SystemVolumeInfoPath,
                Size = 0,
                FileCount = 0,
                Kind = CleanKind.InfoOnly
            };

            try
            {
                if (Directory.Exists(SystemVolumeInfoPath))
                {
                    foreach (string file in EnumerateFiles(SystemVolumeInfoPath, null))
                    {
                        try
                        {
                            item.Size += new FileInfo(file).Length;
                            item.FileCount++;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"统计还原点占用失败: {ex.Message}");
            }

            return item;
        }

        /// <summary>
        /// 扫描WinSxS组件存储
        /// </summary>
        private CleanItem ScanWinSxS()
        {
            // 组件存储只能由 DISM 自己回收；按文件名删 WinSxS 会直接搞坏 Windows  servicing。
            CleanItem item = new CleanItem
            {
                Name = "WinSxS组件存储 — 请执行 Dism /Online /Cleanup-Image /StartComponentCleanup",
                Path = WinSxSPath,
                Size = 0,
                FileCount = 0,
                Kind = CleanKind.InfoOnly
            };

            try
            {
                string output = RunDismAnalyzer();
                item.Size = ParseReclaimableSize(output);

                if (item.Size <= 0)
                {
                    // DISM 需要管理员权限，拿不到结果时退回统计 Backup 目录作为参考值
                    string backupPath = Path.Combine(WinSxSPath, "Backup");
                    foreach (string file in EnumerateFiles(backupPath, null))
                    {
                        try
                        {
                            item.Size += new FileInfo(file).Length;
                            item.FileCount++;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"扫描WinSxS失败: {ex.Message}");
            }

            return item;
        }

        /// <summary>运行 DISM 组件存储分析，失败或超时返回空字符串</summary>
        private static string RunDismAnalyzer()
        {
            try
            {
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "Dism.exe",
                        Arguments = "/Online /Cleanup-Image /AnalyzeComponentStore",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                // 只重定向 stdout，不存在两个管道互相等待的死锁问题
                string output = process.Start() ? process.StandardOutput.ReadToEnd() : "";
                if (!process.WaitForExit(60000))
                {
                    try { process.Kill(entireProcessTree: true); } catch { }
                    return "";
                }
                return output ?? "";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DISM 分析失败: {ex.Message}");
                return "";
            }
        }

        /// <summary>从 DISM 输出里取「可回收大小 / Reclaimable space」，取不到返回 0</summary>
        internal static long ParseReclaimableSize(string output)
        {
            if (string.IsNullOrEmpty(output)) return 0;

            Match m = Regex.Match(output,
                @"(?:可回收(?:空间|大小)?|Reclaimable\s+space)\s*[:：]?\s*([0-9]+(?:[.,][0-9]+)?)\s*(GB|MB|KB|TB|字节|bytes?)",
                RegexOptions.IgnoreCase);
            if (!m.Success) return 0;

            if (!double.TryParse(m.Groups[1].Value.Replace(',', '.'),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double value))
                return 0;

            switch (m.Groups[2].Value.ToUpperInvariant())
            {
                case "TB": return (long)(value * 1024L * 1024 * 1024 * 1024);
                case "GB": return (long)(value * 1024L * 1024 * 1024);
                case "MB": return (long)(value * 1024L * 1024);
                case "KB": return (long)(value * 1024L);
                default: return (long)value;
            }
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
                string windowsOld = WindowsOldPath;
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
                    items.Add(ScanDirectory(windowsOld, "Windows.old (旧系统)", "*.*", CleanKind.WindowsOld));

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

                // 10. 安装程序缓存（只统计不删：这里是 Windows Installer 的补丁源，删掉会导致程序无法修复或卸载）
                scanWorker.ReportProgress(80, "正在扫描安装程序缓存...");
                if (Directory.Exists(installer))
                {
                    CleanItem patchItem = ScanDirectory(installer,
                        "安装程序缓存 — 删除会影响程序修复/卸载，请手动处理", "*.*");
                    patchItem.Kind = CleanKind.InfoOnly;
                    items.Add(patchItem);
                }

                // 11. Windows Defender扫描历史
                scanWorker.ReportProgress(88, "正在扫描Defender历史...");
                string defenderHistory = @"C:\ProgramData\Microsoft\Windows Defender\Scans\History";
                if (Directory.Exists(defenderHistory))
                    items.Add(ScanDirectory(defenderHistory, "Windows Defender扫描历史", "*.*"));

            // 12. 浏览器缓存
            scanWorker.ReportProgress(91, "正在扫描浏览器缓存...");
            items.AddRange(ScanBrowserCaches());

            // 13. 休眠文件（仅统计）
            scanWorker.ReportProgress(94, "正在扫描休眠文件...");
            CleanItem hibernateItem = ScanHibernateFile();
            if (hibernateItem != null && hibernateItem.Size > 0)
                items.Add(hibernateItem);

            // 14. 系统还原点（仅统计）
            scanWorker.ReportProgress(96, "正在统计系统还原点占用...");
            CleanItem restoreItem = ScanSystemRestorePoints();
            if (restoreItem != null && restoreItem.Size > 0)
                items.Add(restoreItem);

            // 15. WinSxS组件存储（仅统计）
            scanWorker.ReportProgress(98, "正在扫描WinSxS组件...");
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

            /// <summary>
            /// 扫描一个目录。Patterns 会原样记在 CleanItem 上，清理阶段复用同一套模式，
            /// 保证「扫到的」与「删掉的」是同一个文件集合（旧实现清理时用 *.* 删整棵树，会误删用户数据）。
            /// </summary>
            private CleanItem ScanDirectory(string path, string name, string patterns, CleanKind kind = CleanKind.Files)
            {
                CleanItem item = new CleanItem
                {
                    Name = name,
                    Path = path,
                    Size = 0,
                    FileCount = 0,
                    Kind = kind,
                    Patterns = SplitPatterns(patterns)
                };

                try
                {
                    if (!Directory.Exists(path)) return item;

                    // 扫描过的根目录即清理白名单；仅统计项不登记，后续无法被清理
                    if (kind != CleanKind.InfoOnly) RegisterRoot(path);

                    foreach (string file in EnumerateFiles(path, item.Patterns))
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

                    _scanned.Add(item);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"扫描目录 {path} 时出错: {ex.Message}");
                }

                return item;
            }

            #endregion

            #region 安全护栏（扫描与删除共用同一套枚举/匹配逻辑）

            private static string[] SplitPatterns(string patterns)
            {
                if (string.IsNullOrWhiteSpace(patterns)) return Array.Empty<string>();
                return patterns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .Where(p => p.Length > 0)
                               .ToArray();
            }

            private void RegisterRoot(string path)
            {
                string full = NormalizePath(path);
                if (full.Length > 0) _roots.Add(full);
            }

            private static string NormalizePath(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) return string.Empty;
                try { path = Path.GetFullPath(path); }
                catch { return string.Empty; }
                return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            /// <summary>
            /// 路径是否落在本次扫描登记的目录之内。这是最后一道闸门：
            /// WinSxS、System Volume Information 等系统目录从不登记，因此永远删不到。
            /// </summary>
            private bool IsCleanablePath(string path)
            {
                string full = NormalizePath(path);
                if (full.Length == 0) return false;

                foreach (string root in _roots)
                {
                    if (string.Equals(full, root, StringComparison.OrdinalIgnoreCase)) return true;
                    if (full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return true;
                }
                return false;
            }

            /// <summary>
            /// 按模式递归枚举文件。与旧实现的 GetFiles(pattern, AllDirectories) 相比：
            /// 1) 每层单独取文件，一个子目录没有权限不会让整个扫描/清理半途抛出；
            /// 2) 跳过重解析点（junction / 挂载点），不会顺着链接删到目录之外的真实数据；
            /// 3) 扫描与清理调用同一个方法，集合天然一致。
            /// </summary>
            private static IEnumerable<string> EnumerateFiles(string root, string[]? patterns)
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                    yield break;

                var stack = new Stack<string>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    string dir = stack.Pop();

                    string[] files;
                    try { files = Directory.GetFiles(dir); }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (DirectoryNotFoundException) { continue; }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"读取目录 {dir} 失败: {ex.Message}");
                        continue;
                    }

                    foreach (string file in files)
                    {
                        if (MatchesPattern(Path.GetFileName(file), patterns))
                            yield return file;
                    }

                    string[] subDirs;
                    try { subDirs = Directory.GetDirectories(dir); }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"读取子目录 {dir} 失败: {ex.Message}");
                        continue;
                    }

                    foreach (string sub in subDirs)
                    {
                        try
                        {
                            if ((File.GetAttributes(sub) & FileAttributes.ReparsePoint) != 0) continue;
                        }
                        catch { /* 读不到属性就当不可进入，宁可少删 */ continue; }
                        stack.Push(sub);
                    }
                }
            }

            private static readonly ConcurrentDictionary<string, Regex> _regexCache =
                new ConcurrentDictionary<string, Regex>();

            /// <summary>文件名是否匹配模式集合。null/空模式表示全部（与扫描时的 * 语义一致）</summary>
            private static bool MatchesPattern(string fileName, string[]? patterns)
            {
                if (patterns == null || patterns.Length == 0) return true;

                foreach (string raw in patterns)
                {
                    string p = raw == null ? string.Empty : raw.Trim();
                    if (p.Length == 0) continue;
                    if (p == "*" || p == "*.*") return true;
                    if (_regexCache.GetOrAdd(p, WildcardToRegex).IsMatch(fileName)) return true;
                }
                return false;
            }

            private static Regex WildcardToRegex(string pattern)
            {
                StringBuilder sb = new StringBuilder("^");
                foreach (char c in pattern)
                {
                    if (c == '*') sb.Append(".*");
                    else if (c == '?') sb.Append('.');
                    else sb.Append(Regex.Escape(c.ToString()));
                }
                return new Regex(sb.Append('$').ToString(),
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }

            #endregion

            #region 扫描线程回调

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
                    // 仅统计项默认不勾选（勾了也会在 StartClean 里被护栏拦下）
                    lvi.Checked = item.Kind != CleanKind.InfoOnly;
                    result.Controls.ListView.Items.Add(lvi);

                    totalSize += item.Size;
                }

                // 交替底色 + 只读项刷灰，让人一眼看出哪些是「只能看、要手动处理」的
                UiTheme.StyleListRows(result.Controls.ListView,
                    it => it.Tag is CleanItem ci && ci.Kind == CleanKind.InfoOnly);

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
                        // 三道护栏：类型允许 + 确为本次扫描产生 + 路径在登记范围内
                        if (item.Kind == CleanKind.InfoOnly)
                        {
                            failedItems.Add($"{item.Name}：该项仅供查看，需按名称中的提示手动处理");
                            continue;
                        }
                        if (!_scanned.Contains(item))
                        {
                            failedItems.Add($"{item.Name}：不是本次扫描产生的项，已拒绝清理");
                            continue;
                        }
                        if (!IsCleanablePath(item.Path))
                        {
                            failedItems.Add($"{item.Name}：路径不在允许清理的范围内（{item.Path}）");
                            continue;
                        }

                        if (item.Kind == CleanKind.WindowsOld)
                        {
                            if (!CleanWindowsOld(item.Path, out string reason))
                                failedItems.Add($"{item.Name}：{reason}");
                        }
                        else
                        {
                            DeleteMatchedFiles(item, failedItems);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"清理 {item.Name} 时出错: {ex.Message}");
                        failedItems.Add($"{item.Name}：{ex.Message}");
                    }
                }

                e.Result = new CleanResult
                {
                    FailedItems = failedItems,
                    Controls = data
                };
            }

            /// <summary>
            /// 只删除扫描时按 Patterns 记下的那批文件（旧实现用 "*.*" + AllDirectories 删整棵树，
            /// 会连用户放在临时/缓存目录里的真实文件一起删掉）。不再删除任何目录。
            /// </summary>
            private void DeleteMatchedFiles(CleanItem item, List<string> failedItems)
            {
                if (!Directory.Exists(item.Path))
                {
                    failedItems.Add($"{item.Name}：目录已不存在（{item.Path}）");
                    return;
                }

                int deleted = 0, failed = 0;
                long bytes = 0;
                string firstError = null;

                foreach (string file in EnumerateFiles(item.Path, item.Patterns))
                {
                    try
                    {
                        long length = 0;
                        try { length = new FileInfo(file).Length; } catch { }

                        // 只读/系统/隐藏属性会直接让 Delete 抛 UnauthorizedAccessException
                        try { File.SetAttributes(file, FileAttributes.Normal); } catch { }

                        File.Delete(file);
                        deleted++;
                        bytes += length;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        if (firstError == null) firstError = ex.Message;
                        Debug.WriteLine($"删除文件 {file} 失败: {ex.Message}");
                    }
                }

                item.DeletedFiles = deleted;
                item.DeletedBytes = bytes;
                item.FailedFiles = failed;

                if (failed > 0)
                {
                    string tail = firstError == null ? "" : $"（示例：{firstError}）";
                    failedItems.Add(deleted == 0
                        ? $"{item.Name}：{failed} 个文件全部未能删除，可能被占用或权限不足{tail}"
                        : $"{item.Name}：已删 {deleted} 个，另有 {failed} 个被占用或权限不足{tail}");
                }
            }

            /// <summary>
            /// 用 takeown + icacls + rd 删除 Windows.old。
            /// 旧写法把 Verb="runas" 和 UseShellExecute=false 写在一起，Verb 会被静默忽略（提权根本没发生），
            /// 而且超时后继续读 ExitCode 会抛 InvalidOperationException。路径先收敛到固定的 Windows.old。
            /// </summary>
            private bool CleanWindowsOld(string path, out string reason)
            {
                reason = string.Empty;

                string full = NormalizePath(path);
                if (!string.Equals(full, NormalizePath(WindowsOldPath), StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"为安全起见只允许删除固定的 {WindowsOldPath}";
                    return false;
                }
                if (!CheckAdminPrivileges())
                {
                    reason = "需要管理员权限，请以管理员身份重新运行本程序";
                    return false;
                }

                try
                {
                    using Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            // 不重定向任何管道（CreateNoWindow 下输出直接丢弃），避免双管道互等死锁
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };
                    // 路径已限定为 WindowsOldPath 常量，不存在拼接注入面
                    process.StartInfo.ArgumentList.Add("/c");
                    process.StartInfo.ArgumentList.Add(
                        $"takeown /F \"{full}\" /R /D Y && icacls \"{full}\" /grant administrators:F /T && rd /s /q \"{full}\"");

                    if (!process.Start())
                    {
                        reason = "无法启动 cmd.exe";
                        return false;
                    }
                    if (!process.WaitForExit(180000))
                    {
                        try { process.Kill(entireProcessTree: true); } catch { }
                        reason = "删除超时（超过 180 秒），已终止";
                        return false;
                    }

                    int code = process.ExitCode;
                    if (code != 0) reason = $"命令返回错误码 {code}，可能仍有文件被占用";
                    return code == 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"清理Windows.old失败: {ex.Message}");
                    reason = ex.Message;
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

                // 真实的删除统计，不再无论删没删都弹“全部成功”
                int deletedFiles = 0, failedFiles = 0;
                long deletedBytes = 0;
                foreach (var it in result.Controls.Items)
                {
                    deletedFiles += it.DeletedFiles;
                    deletedBytes += it.DeletedBytes;
                    failedFiles += it.FailedFiles;
                }

                string summary = $"已删除 {deletedFiles} 个文件，释放约 {FormatSize(deletedBytes)}。";

                if (result.FailedItems.Count > 0)
                {
                    string failedList = string.Join("\n", result.FailedItems.Take(12));
                    if (result.FailedItems.Count > 12)
                        failedList += $"\n… 以及另外 {result.FailedItems.Count - 12} 条";

                    MessageBox.Show($"{summary}\n\n以下 {result.FailedItems.Count} 项未完全成功：\n\n{failedList}\n\n" +
                        "通常是文件正被占用或权限不足；建议关闭相关程序、以管理员身份重试。",
                        "清理完成（部分失败）", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (deletedFiles == 0)
                {
                    MessageBox.Show("没有删掉任何文件：这些位置里本次没有符合类型的文件，或已全部不存在。",
                        "清理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(summary + "\n所有选中项均已处理完毕。", "成功",
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
        /// <summary>
        /// 清理项类型——决定清理阶段如何处理它
        /// </summary>
        public enum CleanKind
        {
            /// <summary>在登记目录下按 Patterns 删除文件</summary>
            Files,
            /// <summary>Windows.old，需 takeown/icacls/rd</summary>
            WindowsOld,
            /// <summary>只展示统计，本工具永不删（休眠文件/还原点/WinSxS/Installer 缓存）</summary>
            InfoOnly
        }

        /// <summary>
        /// 清理项数据模型
        /// </summary>
        public class CleanItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public long Size { get; set; }
            public int FileCount { get; set; }

            /// <summary>扫描时使用的文件匹配模式，清理时严格复用（保证扫删一致）</summary>
            public string[] Patterns { get; set; } = Array.Empty<string>();

            public CleanKind Kind { get; set; } = CleanKind.Files;

            /// <summary>本次清理实际删掉的文件数/字节数、以及删不掉的个数</summary>
            public int DeletedFiles { get; set; }
            public long DeletedBytes { get; set; }
            public int FailedFiles { get; set; }
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
            /// <summary>失败项描述，已带上原因（旧实现只记名字，用户不知道为什么失败）</summary>
            public List<string> FailedItems { get; set; }
            public CleanData Controls { get; set; }
        }



       
    

        #endregion
    }






