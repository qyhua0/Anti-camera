/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：util/NpcapNetworkMonitor.cs
 * 作者：zhenhua
 * 说明：基于 SharpPcap/Npcap 的实时流量监控：抓包、按五元组聚合连接、按进程统计上下行字节，
 * 并用 IPHelper + 进程名把结果喂给界面表格。需要管理员权限且已安装 Npcap。
 */
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using SharpPcap;
using PacketDotNet;
using WinDisplay.util;
using System.Windows.Forms;


/// <summary>
/// 使用 Npcap 监控网络流量，按进程统计连接和带宽。
/// 必须以管理员权限运行，并安装 Npcap。
/// <para>
/// 工作方式：全局单例（全静态）——<see cref="Start"/> 在一块网卡上开 SharpPcap 的抓包线程，
/// <see cref="Device_OnPacketArrival"/> 把包累加到连接表，另一个定时器按 <c>refreshMs</c>
/// 调 <see cref="UpdateUI"/> 把增量换算成速率刷表格，所以表格上看到的不是包而是「每连接每秒」。
/// 界面控件（DataGridView）由调用方传进来，本类不建窗体。
/// </para>
/// </summary>
public static class NpcapNetworkMonitor

{

    // ===== 新增字段 =====
    /// <summary>表格的数据源。抓包线程只写集合元素，不增删行，配合 <see cref="_uiUpdating"/> 避免跨线程改集合。</summary>
    private static BindingList<ConnView> _binding;
    /// <summary>绑到 DataGridView 上的中转层，用来做当前行定位，不自己写 Binding 代码。</summary>
    private static BindingSource _bs;
    /// <summary>连接键 -> 表格行，速率靠它「同一个连接累加同一行」而不是每轮重建列表。键的拼法见 <see cref="Device_OnPacketArrival"/>。</summary>
    private static readonly Dictionary<string, ConnView> _viewIndex = new(); // key -> ConnView
    /// <summary>UI 刷新中标记（0 空闲 / 1 正在刷），用 <c>Interlocked</c> .exchange 拿，防止上一轮没刷完又开始下一轮。</summary>
    private static int _uiUpdating = 0; // 0: idle, 1: updating

    /// <summary>行右键菜单（看进程文件位置 / 结束进程 / 屏蔽这个 IP 等），在 <see cref="InitRowContextMenu"/> 里建。</summary>
    private static ContextMenuStrip _rowMenu;
    /// <summary>右键时选中的行数据：菜单项回调拿不到行，只能先把上下文存在这里。</summary>
    private static ConnView _ctxRow; // 右键时选中的行数据


    // ===== 工具：给 DGV 开启双缓冲（WinForms 反射）=====
    /// <summary>
    /// 每秒全量刷新几万行时，不双缓冲就能看到表格一条条抽。DataGridView 的
    /// <c>DoubleBuffered</c> 是 protected 属性，只能用反射写。
    /// </summary>
    private static void EnableDoubleBuffer(DataGridView dgv)
    {
        typeof(DataGridView).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
            null, dgv, new object[] { true });
    }


    /// <summary>
    /// 拆地址串：支持 <c>1.2.3.4:80</c> 与 IPv6 的 <c>[2001:db8::1]:443</c>。
    /// IPv6 里本身就有冒号，所以必须取最后一个冒号（或方括号后那个），不能直接 Split(':')。
    /// </summary>
    /// <returns>解析不出端口就返回 false，不抛异常（日志里脏数据很多）。</returns>
    private static bool TryParseEndpoint(string endpoint, out string ip, out int port)
    {
        ip = null; port = 0;
        if (string.IsNullOrWhiteSpace(endpoint)) return false;

        // 形如 "1.2.3.4:80" 或 "[2001:db8::1]:443"
        try
        {
            if (endpoint.StartsWith("["))
            {
                int r = endpoint.IndexOf(']');
                if (r > 0)
                {
                    ip = endpoint.Substring(1, r - 1);
                    int c = endpoint.IndexOf(':', r + 1);
                    if (c > 0) int.TryParse(endpoint.Substring(c + 1), out port);
                    return true;
                }
            }
            else
            {
                int c = endpoint.LastIndexOf(':');
                if (c > 0)
                {
                    ip = endpoint.Substring(0, c);
                    int.TryParse(endpoint.Substring(c + 1), out port);
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    /// <summary>右键菜单「打开文件位置」：用 <c>explorer /select</c> 在资源管理器里选中这个文件（不是只打开目录）。</summary>
    private static void OpenFileLocation(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                MessageBox.Show("无法定位文件：路径不存在或无权限。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using Process? explorer = Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("打开文件位置失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>右键菜单用默认浏览器打开一个网址（例如查这个远端 IP 的归属信息）。</summary>
    private static void OpenUrl(string url)
    {
        try
        {
            using Process? browser = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("打开浏览器失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 右键菜单「结束进程」：<c>entireProcessTree: true</c> 连子进程一起结束——
    /// 很多下载器/注入器靠子进程拉活，只杀父进程等于没杀。
    /// </summary>
    private static void KillProcess(int pid)
    {
        try
        {
            using var p = Process.GetProcessById(pid);
            p.Kill(entireProcessTree: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"结束进程失败：{ex.Message}\r\n请确认是否有管理员权限。", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>当前进程是否带管理员令牌。结束别人的进程、改防火墙都需要它，不满足就提前提示而不是静默失败。</summary>
    private static bool IsAdministrator()
    {
        try
        {
            var wi = System.Security.Principal.WindowsIdentity.GetCurrent();
            var wp = new System.Security.Principal.WindowsPrincipal(wi);
            return wp.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch { return false; }
    }

    /// <summary>
    /// 屏蔽某个远程 IP（入站 + 出站各一条规则）。规则名固定成
    /// <c>NpcapBlock_IP_IN/OUT_&lt;ip&gt;</c>，一是人能在防火墙里认出是谁加的，二是重名时 netsh 会直接报错而不是默默加重复规则。
    /// </summary>
    private static void FirewallBlockIp(string ip)
    {
        if (!IsAdministrator())
        {
            MessageBox.Show("需要管理员权限才能修改防火墙规则。", "权限不足",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // 规则名尽量可读，避免重复
            string ruleIn = $"NpcapBlock_IP_IN_{ip}";
            string ruleOut = $"NpcapBlock_IP_OUT_{ip}";

            // 先添加出站
            RunNetsh($@"advfirewall firewall add rule name=""{ruleOut}"" dir=out action=block remoteip={ip}");
            // 再添加入站
            RunNetsh($@"advfirewall firewall add rule name=""{ruleIn}"" dir=in action=block remoteip={ip}");

            MessageBox.Show($"已添加防火墙规则，阻止与 {ip} 的入站/出站流量。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("添加防火墙规则失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 屏蔽某个可执行文件的入站/出站。比按 IP 屏蔽更适合「不让这个程序联网」这种需求，
    /// 但只对路径固定的程序有效：换了目录或改名，规则就跟不上了（没做哈希规则，netsh 也不支持）。
    /// </summary>
    private static void FirewallBlockApp(string exePath)
    {
        if (!IsAdministrator())
        {
            MessageBox.Show("需要管理员权限才能修改防火墙规则。", "权限不足",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show("可执行文件不存在。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string ruleOut = $"NpcapBlock_APP_OUT_{Path.GetFileName(exePath)}";
            string ruleIn = $"NpcapBlock_APP_IN_{Path.GetFileName(exePath)}";

            RunNetsh($@"advfirewall firewall add rule name=""{ruleOut}"" dir=out action=block program=""{exePath}"" enable=yes");
            RunNetsh($@"advfirewall firewall add rule name=""{ruleIn}"" dir=in action=block program=""{exePath}"" enable=yes");

            MessageBox.Show($"已阻止 {exePath} 的入站/出站网络访问。", "完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("添加防火墙规则失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 跑一次 <c>netsh advfirewall</c>，非 0 退出码直接抛异常带 stderr，调用方统一提示。
    /// <para>
    /// 两个管道必须并行读（下面已注明原因）；<c>WaitForExit(15000)</c> 超时就杀掉整棵进程树，
    /// 不能把 UI 线程挂在 UAC 上。ExitCode 只能在确认退出后读。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 坑：下面写的 <c>Verb = "runas"</c> 在 <c>UseShellExecute = false</c> 时是被系统忽略的，
    /// 所以非管理员跑到这里不会弹 UAC，而是 netsh 以受限身份失败（非 0 退出码）。
    /// 真正要靠的是程序自带的管理员入口；改这里之前先看 app.manifest。
    /// </remarks>
    /// <exception cref="InvalidOperationException">netsh 返回非 0，消息里带退出码与 stderr/stdout。</exception>
    /// <exception cref="TimeoutException">15 秒没退完（包括 UAC 弹窗没人点）。</exception>
    private static void RunNetsh(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Verb = IsAdministrator() ? "" : "runas" // 已是管理员则不再弹 UAC
        };
        using var p = Process.Start(psi)
            ?? throw new InvalidOperationException("无法启动 netsh.exe");

        // 两个管道必须异步读。原来先 ReadToEnd(stdout) 再 ReadToEnd(stderr)：
        // 只要 netsh 往 stderr 先写满管道缓冲区（约 4KB），它阻塞在写、我们阻塞在读，死锁
        Task<string> outTask = p.StandardOutput.ReadToEndAsync();
        Task<string> errTask = p.StandardError.ReadToEndAsync();

        if (!p.WaitForExit(15000))
        {
            try { p.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException("netsh 执行超时（15 秒），已终止。如果是权限弹窗没点确认，请重试。");
        }

        int exitCode = p.ExitCode;   // 只能在确认进程已退出之后读，原来超时时也照读不误，抽到的异常和真实原因无关
        string stdout = PipeResult(outTask);
        string stderr = PipeResult(errTask);

        if (exitCode != 0)
            throw new InvalidOperationException($"netsh 失败（退出码 {exitCode}）：{stderr}\r\n{stdout}");
    }

    /// <summary>进程退出后管道里的内容可能还没读完，给它两秒；拿不到就当空，不能卡死调用线程。</summary>
    private static string PipeResult(Task<string> readTask)
    {
        try { return readTask.Wait(2000) && readTask.IsCompletedSuccessfully ? readTask.Result : string.Empty; }
        catch { return string.Empty; }
    }


    /// <summary>
    /// 给表格建一次行右键菜单：看进程文件位置、打开网页、结束进程、屏蔽这个 IP / 这个程序。
    /// 菜单项回调只能拿到 <see cref="_ctxRow"/>，所以下面先用 MouseDown 把右键命中的行存起来。
    /// </summary>
    private static void InitRowContextMenu(DataGridView grid)
    {
        if (grid == null || grid.IsDisposed) return;

        // 右键时选中所在行
        grid.MouseDown += (s, e) =>
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = grid.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.Cell && hit.RowIndex >= 0)
            {
                grid.ClearSelection();
                grid.CurrentCell = grid.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                grid.Rows[hit.RowIndex].Selected = true;
                _ctxRow = grid.Rows[hit.RowIndex].DataBoundItem as ConnView;
            }
            else
            {
                _ctxRow = null;
            }
        };

        // 构建菜单
        _rowMenu = new ContextMenuStrip();

        var miOpenFolder = new ToolStripMenuItem("定位文件路径");
        miOpenFolder.Click += (s, e) => { if (_ctxRow != null) OpenFileLocation(_ctxRow.FilePath); };

        var miSearchIp = new ToolStripMenuItem("搜索远程IP");
        miSearchIp.Click += (s, e) =>
        {
            if (_ctxRow == null) return;
            if (TryParseEndpoint(_ctxRow.Remote, out var ip, out _))
                OpenUrl($"https://www.bing.com/search?q={Uri.EscapeDataString(ip)}");
        };

        var miKill = new ToolStripMenuItem("结束进程");
        miKill.Click += async (s, e) =>
        {
            if (_ctxRow == null) return;
            if (MessageBox.Show($"结束进程 {_ctxRow.ProcessName} (PID={_ctxRow.PID})？",
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                await Task.Run(() => KillProcess(_ctxRow.PID));
            }
        };

        var miFirewall = new ToolStripMenuItem("防火墙阻塞");
        var miFwBlockIp = new ToolStripMenuItem("阻止远程IP（入站/出站）");
        miFwBlockIp.Click += async (s, e) =>
        {
            if (_ctxRow == null) return;
            if (TryParseEndpoint(_ctxRow.Remote, out var ip, out _))
                await Task.Run(() => FirewallBlockIp(ip));
        };

        var miFwBlockApp = new ToolStripMenuItem("阻止该进程网络（按可执行文件）");
        miFwBlockApp.Click += async (s, e) =>
        {
            if (_ctxRow == null) return;
            var exe = _ctxRow.FilePath;
            if (!string.IsNullOrWhiteSpace(exe))
                await Task.Run(() => FirewallBlockApp(exe));
        };
        miFirewall.DropDownItems.Add(miFwBlockIp);
        miFirewall.DropDownItems.Add(miFwBlockApp);

        _rowMenu.Items.Add(miOpenFolder);
        _rowMenu.Items.Add(new ToolStripSeparator());
        _rowMenu.Items.Add(miSearchIp);
        _rowMenu.Items.Add(new ToolStripSeparator());
        _rowMenu.Items.Add(miKill);
        _rowMenu.Items.Add(new ToolStripSeparator());
        _rowMenu.Items.Add(miFirewall);

        // 根据行数据启用/禁用菜单项
        _rowMenu.Opening += (s, e) =>
        {
            var hasRow = _ctxRow != null;
            miOpenFolder.Enabled = hasRow && !string.IsNullOrWhiteSpace(_ctxRow.FilePath);
            miSearchIp.Enabled = hasRow && TryParseEndpoint(_ctxRow.Remote, out _, out _);
            miKill.Enabled = hasRow && _ctxRow.PID > 0;
            miFirewall.Enabled = hasRow;
            miFwBlockIp.Enabled = hasRow && TryParseEndpoint(_ctxRow.Remote, out _, out _);
            miFwBlockApp.Enabled = hasRow && !string.IsNullOrWhiteSpace(_ctxRow.FilePath);
        };

        grid.ContextMenuStrip = _rowMenu;
    }


    // --- 公共 Start/Stop 接口 ---
    /// <summary>
    /// 在指定网卡上开始抓包并把结果刷到表格里。全局单例：第一件事是 <see cref="Stop"/> 旧实例，
    /// 所以重复点「开始」不会开两个抓包线程。
    /// </summary>
    /// <param name="dgw">目标表格。列由 <c>AutoGenerateColumns</c> 从 <see cref="ConnView"/> 的属性生成，
    /// 本方法顺手把列宽、排序、满行选中、双缓冲、右键菜单配好（全部在 UI 线程做一次）。</param>
    /// <param name="deviceIndex">网卡序号，必须跟调用方下拉框用的 <c>CaptureDeviceList</c> 枚举顺序一致；
    /// 越界不报错，静默回到 0。</param>
    /// <param name="refreshMs">界面刷新周期，下限 200ms（再小就是自己做 DoS）。</param>
    /// <param name="csvLogPath">逐轮快照的 CSV 落地路径，null 回到默认 <c>net_npcap_log.csv</c>。</param>
    /// <remarks>
    /// 设备开混杂模式 + 1000ms 读超时，包过滤器只留 <c>ip and (tcp or udp)</c>，
    /// 所以 ARP / 非 IP 流量不进统计；设备打不开时统一包成 <see cref="InvalidOperationException"/>，
    /// 消息里带上 Npcap / 管理员权限的提示，给 Form_net 直接弹。不预清表格数据：刷新周期内到达的包
    /// 会直接建新行。
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="dgw"/> 为空。</exception>
    /// <exception cref="InvalidOperationException">没枚举到抓包设备（未装 Npcap / 未提权），或设备打不开。</exception>
    public static void Start(DataGridView dgw, int deviceIndex = 0, int refreshMs = 1000, string csvLogPath = "net_npcap_log.csv")
    {
        Stop(); // 先确保停止旧实例
        if (dgw == null) throw new ArgumentNullException(nameof(dgw));
        _grid = dgw;
        _refreshMs = Math.Max(200, refreshMs);
        _csvLogPath = csvLogPath ?? "net_npcap_log.csv";
        EnsureLogHeader();

        // 绑定 DataGridView（先空数据源）
        _grid.InvokeIfRequired(() =>
        {
            // 只做一次性初始化
            _binding = new BindingList<ConnView>();
            _bs = new BindingSource { DataSource = _binding };
            _grid.DataSource = _bs;

            // 用户可拖动列宽；禁用自动列宽，防止每次刷新改列宽
            _grid.AutoGenerateColumns = true; // 首次由属性自动建列
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _grid.AllowUserToResizeColumns = true;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // 只锁头高，不锁列宽
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;


           
          

            // 列宽只在第一次初始化时给个基础值（存在就不覆盖）
            foreach (DataGridViewColumn col in _grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                if (col.Width < 60) col.Width = 100;
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }


            // 设置其他列自动填充
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            // 指定列宽度，不自动填充
            SetColWidth(_grid, "PID", 60);
            SetColWidth(_grid, "Protocol", 80);
            SetColWidth(_grid, "ProcessName", 160);
            SetColWidth(_grid, "FilePath", 550);




            InitRowContextMenu(_grid);


            EnableDoubleBuffer(_grid);
        });


        // 选择设备
        var devices = CaptureDeviceList.Instance;
        if (devices == null || devices.Count == 0)
        {
            throw new InvalidOperationException("未检测到任何抓包设备，请安装 Npcap 并以管理员权限运行。");

        }

        if (deviceIndex < 0 || deviceIndex >= devices.Count) deviceIndex = 0;
        _device = devices[deviceIndex];

        try
        {

            // 订阅事件并启动
             _device.OnPacketArrival += Device_OnPacketArrival;


            // 使用正确的重载：设置混杂模式和读取超时
            _device.Open(DeviceModes.Promiscuous, 1000); // 1000ms 读取超时
            _device.Filter = "ip and (tcp or udp)"; // 只抓取 TCP/UDP 的 IP 包
            _device.StartCapture();


           // UI 刷新定时器（线程池）
            _uiTimer = new System.Threading.Timer(_ => UpdateUI(), null, _refreshMs, _refreshMs);
            _running = true;

            Debug.WriteLine($"[Npcap] 开始监控设备: {_device.Description}");
        }
        catch (Exception ex)
        {
            Stop();
            throw new InvalidOperationException($"无法启动抓包设备: {ex.Message}", ex);
        }
    }

    /// <summary>把某列从自动填充（Fill）里挑出来固定宽度。整体是 Fill 模式，不这么做 PID/Protocol 会被挤成一条。</summary>
    private static void SetColWidth(DataGridView grid, string colName, int width)
    {
        grid.Columns[colName].Width = width; 
        //grid.Columns[colName].FillWeight = 1; // 可选：防止 Fill 影响它（但实际不会，因为不是 Fill 模式）                               
        grid.Columns[colName].AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // 列不参与自动填充（关键：将其 AutoSizeMode 设置为 None）
    }

    /// <summary>
    /// 停止抓包：停线程 → 关设备 → 退订事件 → 释放刷新定时器 → 清统计与进程名缓存。
    /// <para>
    /// 每一步失败都只写 Debug 日志不抛：关窗/退出时这里再报异常只会把退出流程打断，而大多数情况
    /// 本来就是「已经关了又调一次」。表格里的行故意留着（下面那段清空 UI 的代码是被有意注掉的），
    /// 让人停止之后还能看清最后一轮连了哪些地址。
    /// </para>
    /// </summary>
    public static void Stop()
    {
        _running = false;
        try
        {
            if (_device != null)
            {
                try { _device.StopCapture(); } catch (Exception ex) { Debug.WriteLine($"[Npcap] StopCapture error: {ex.Message}"); }
                try { _device.Close(); } catch (Exception ex) { Debug.WriteLine($"[Npcap] Close error: {ex.Message}"); }
                _device.OnPacketArrival -= Device_OnPacketArrival;
                _device = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Npcap] Stop error: {ex.Message}");
        }

        if (_uiTimer != null)
        {
            try { _uiTimer.Dispose(); } catch (Exception ex) { Debug.WriteLine($"[Npcap] Timer Dispose error: {ex.Message}"); }
            _uiTimer = null;
        }

        lock (_lock)
        {
            _stats.Clear();
            _processCache.Clear(); // 清理缓存
        }

        // 清 UI
        //if (_grid != null)
        //{
        //    _grid.InvokeIfRequired(() =>
        //    {
        //        if (!_grid.IsDisposed)
        //        {
        //            // 不重建 DataSource，只清空数据，保留列与列宽
        //            _bs?.SuspendBinding();
        //            _binding?.Clear();
        //            _viewIndex.Clear();
        //            _bs?.ResumeBinding();
        //            // 不调用 _grid.Refresh(); 留给 WinForms 自己重绘
        //        }
        //    });
        //}

        Debug.WriteLine("[Npcap] 监控已停止");
    }

    // --- 内部数据结构 ---
    /// <summary>
    /// 一条连接的原始账本（抓包线程只改它），按刷新周期换算出速率。<see cref="ConnView"/> 才是给表格看的那一份。
    /// </summary>
    private class ConnInfo
    {
        /// <summary>包所属进程（由本地端口反查，见 <see cref="PortToPID"/>）；查不到为 0。</summary>
        public int PID;
        /// <summary>进程名，走 <see cref="_processCache"/>，同一个 PID 只查一次。</summary>
        public string ProcessName;
        /// <summary>可执行文件完整路径，取不到（系统进程/无权限）为串。</summary>
        public string FilePath;
        public string Protocol; // "TCP"/"UDP"
        /// <summary>本地地址（已统一成字符串，IPv6 不带方括号）。</summary>
        public string LocalIP;
        public int LocalPort;
        /// <summary>对端地址。</summary>
        public string RemoteIP;
        public int RemotePort;
        /// <summary>累计发出字节（含包头，不是净载荷）。</summary>
        public long BytesSent;
        /// <summary>累计收到字节（含包头）。</summary>
        public long BytesRecv;
        /// <summary>上次累计的总字节数，用于计算速率</summary>
        public long LastTotal; // 上次累计的总字节数，用于计算速率
        /// <summary>当前速率 (Bytes per second)，= (总字节 - LastTotal) / 经过的秒数，只在刷新时更新。</summary>
        public long SpeedBps;  // 当前速率 (Bytes per second)
    }

    /// <summary>
    /// 表格一行。字段名就是列头（<c>AutoGenerateColumns</c>），所以只能加不能改名；
    /// <see cref="Local"/> / <see cref="Remote"/> 已经把地址和端口合成了一个串，因为用户看的是
    /// <c>1.2.3.4:443</c> 而不是两个数。
    /// </summary>
    public class ConnView
    {
        /// <summary>列「ProcessName」：进程名。</summary>
        public string ProcessName { get; set; }
        /// <summary>列「PID」：进程序号，为 0 表示没能把包归到某个进程。</summary>
        public int PID { get; set; }
        /// <summary>列「FilePath」：可执行文件路径，右键菜单靠它定位文件 / 屏蔽这个程序。</summary>
        public string FilePath { get; set; }
        /// <summary>列「Protocol」：TCP 或 UDP。</summary>
        public string Protocol { get; set; }
        /// <summary>列「Local」：本地 <c>ip:port</c>。</summary>
        public string Local { get; set; }
        /// <summary>列「Remote」：对端 <c>ip:port</c>，屏蔽 IP 与查归属都用它。</summary>
        public string Remote { get; set; }
        /// <summary>列「BytesSent」：累计发送字节。</summary>
        public long BytesSent { get; set; }
        /// <summary>列「BytesRecv」：累计接收字节。</summary>
        public long BytesRecv { get; set; }
        /// <summary>列「SpeedBps」：当前速率，字节/秒。</summary>
        public long SpeedBps { get; set; }
    }

    // --- 字段 ---
    /// <summary>当前抓包设备；null 表示没在抓。</summary>
    private static ICaptureDevice _device;
    /// <summary>目标表格（由 <see cref="Start"/> 传入），刷新一律走 <c>InvokeIfRequired</c>。</summary>
    private static DataGridView _grid;
    /// <summary>线程池定时器，每 <see cref="_refreshMs"/> 调一次 <see cref="UpdateUI"/>；它在非 UI 线程上跑。</summary>
    private static System.Threading.Timer _uiTimer;
    /// <summary>守 <see cref="_stats"/> 与 <see cref="_processCache"/>：抓包线程在写，刷新线程在读。</summary>
    private static readonly object _lock = new object();
    /// <summary>连接键 -> 原始账本，键的拼法见 <see cref="Device_OnPacketArrival"/>。</summary>
    private static readonly Dictionary<string, ConnInfo> _stats = new Dictionary<string, ConnInfo>();
    /// <summary>PID -> (进程名, 路径)。每个包都查一次进程信息会把抓包线程拖死，所以按 PID 缓存。</summary>
    private static readonly Dictionary<int, (string name, string path)> _processCache = new Dictionary<int, (string, string)>();
    /// <summary>抓包进行中标志，事件回调里靠它提前退出（Stop 之后可能还有包在飞）。</summary>
    private static bool _running = false;
    /// <summary>刷新周期毫秒，<see cref="Start"/> 里已做 200ms 下限。</summary>
    private static int _refreshMs = 1000;
    /// <summary>CSV 快照落地路径。</summary>
    private static string _csvLogPath = "net_npcap_log.csv";

    // --- 事件回调 ---
    /// <summary>
    /// 每个抓到的包都会进来一次，跑在 SharpPcap 自己的抓包线程上（不是 UI 线程）。
    /// <para>
    /// 流程：取原始字节 → <c>Packet.ParsePacket</c> 按链路层解 → 取 IP 头 → 取 TCP/UDP 头拿端口 →
    /// <see cref="PortToPID"/> 把包归到进程 → 用 <c>pid_协议_本地地址_对端地址</c> 做键累加字节。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 几个必须知道的取舍：
    /// 一是字节数按 <c>rawData.Length</c> 累加，那是拓到的整个帧的长度（含链路层/IP/传输头），
    /// 比应用层的净载荷大一些，跟任务管理器对不上属正常；
    /// 二是 <c>pid &lt;= 0</c> 的包直接丢掉，所以表里看不到「不知道是谁发的」；
    /// 三是整段包了 catch，一个解坏的包不能把抓包线程带倒；
    /// 四是下面那几行按包写的 <c>Debug.WriteLine</c> 是调试残留，流量大的时候它比解包本身还贵，
    /// 要真实跑性先把它去掉（只在调试器附加时才有输出，但仍要拼字符串）。
    /// </remarks>
    private static void Device_OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {

            Debug.WriteLine("[Npcap] Device_OnPacketArrival 被调用！");

            // 为了更明显，可以弹出一个 MessageBox（只测试用，记得删除）
            // Task.Run(() => MessageBox.Show("抓到包了！", "Debug", MessageBoxButtons.OK)); // 注意：不要在事件里直接弹，用Task

            var raw = e.GetPacket(); // RawCapture
            if (raw == null)
            {
                Debug.WriteLine("[Npcap] raw 为 null");
                return;
            }

            // 修复：安全地获取 rawData (byte[])
            byte[] rawData = null;
            try
            {
                // 尝试直接获取（旧版 SharpPcap）
                rawData = raw.Data;
                Debug.WriteLine($"[Npcap] 抓到一个数据包，长度: {raw.Data.Length} 字节");

            }
            catch
            {
                // 新版 SharpPcap 可能返回 ReadOnlyMemory<byte>
                var dataProp = raw.GetType().GetProperty("Data");
                if (dataProp != null)
                {
                    var dataValue = dataProp.GetValue(raw);
                    if (dataValue is byte[] bytes)
                    {
                        rawData = bytes;
                    }
                    else if (dataValue is ReadOnlyMemory<byte> memory)
                    {
                        rawData = memory.ToArray();
                    }
                }
            }

            if (rawData == null || rawData.Length == 0) return;

            // 解析 IP 包
            var packet = Packet.ParsePacket(raw.LinkLayerType, rawData);
            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket == null) return;

            var srcIp = ipPacket.SourceAddress.ToString();
            var dstIp = ipPacket.DestinationAddress.ToString();

            string proto = null;
            int srcPort = 0, dstPort = 0;

            var tcpPacket = packet.Extract<TcpPacket>();
            if (tcpPacket != null)
            {
                proto = "TCP";
                srcPort = (int)tcpPacket.SourcePort;
                dstPort = (int)tcpPacket.DestinationPort;
            }
            else
            {
                var udpPacket = packet.Extract<UdpPacket>();
                if (udpPacket != null)
                {
                    proto = "UDP";
                    srcPort = (int)udpPacket.SourcePort;
                    dstPort = (int)udpPacket.DestinationPort;
                }
            }

            if (proto == null)
            {
                return; // 不是 TCP/UDP
            }

            Debug.WriteLine($"[Npcap] 解析数据包1: {proto}");


            // 使用端口->PID 映射
            bool isOutbound;
            int pid = PortToPID(proto, srcIp, srcPort, dstIp, dstPort, out isOutbound);
            if (pid <= 0) return; // 无法归属
            Debug.WriteLine($"[Npcap] 解析数据包2: {proto}");


            // 生成唯一键 (包含方向)：同一对地址的进/出方向分开记，不然 BytesSent/BytesRecv 会互相串
            string localIp = isOutbound ? srcIp : dstIp;
            int localPort = isOutbound ? srcPort : dstPort;
            string remoteIp = isOutbound ? dstIp : srcIp;
            int remotePort = isOutbound ? dstPort : srcPort;
            string key = $"{pid}_{proto}_{localIp}:{localPort}_{remoteIp}:{remotePort}";

            long pktBytes = rawData.Length;

            lock (_lock)
            {
                if (!_stats.TryGetValue(key, out var ci))
                {
                    // 从缓存或系统获取进程信息
                    if (!_processCache.TryGetValue(pid, out var procInfo))
                    {
                        procInfo = GetProcessNameAndPath(pid);
                        _processCache[pid] = procInfo;
                    }

                    ci = new ConnInfo
                    {
                        PID = pid,
                        ProcessName = procInfo.name,
                        FilePath = procInfo.path,
                        Protocol = proto,
                        LocalIP = localIp,
                        LocalPort = localPort,
                        RemoteIP = remoteIp,
                        RemotePort = remotePort,
                        BytesSent = 0,
                        BytesRecv = 0,
                        LastTotal = 0,
                        SpeedBps = 0
                    };
                    _stats[key] = ci;
                }

                if (isOutbound)
                    ci.BytesSent += pktBytes;
                else
                    ci.BytesRecv += pktBytes;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Npcap] 解析数据包时出错: {ex.Message}");
        }
    }

    // --- UI 更新 ---
    /// <summary>
    /// 每个刷新周期跑一次，把 <see cref="_stats"/> 换算成速率并增量同步到表格。
    /// 跑在线程池定时器上，不是 UI 线程。
    /// <para>
    /// 三步：加 <see cref="_lock"/> 拍快照（顺手算速率）→ <c>InvokeIfRequired</c> 回 UI 线程做增量
    /// 比对（新增行才 Add，已存在的只改变了的那个字段，消失的连接才 Remove）→ 写 CSV。
    /// 不重建 <c>DataSource</c>，也不调 <c>Refresh()</c>：否则每秒整表重建，滚动条与选中行都会跳。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 重入靠 <c>Interlocked.Exchange(_uiUpdating, 1)</c>：上一轮没跑完就直接放弃这一 tick（丢一帧，
    /// 不排队），否则卡一次会攒出一堆补刷。
    /// 速率用的是配置里的 <c>_refreshMs</c> 而不是实际经过的时间，所以定时器被堵时这一格速率会偏低。
    /// 字符串字段用 <c>ReferenceEquals</c> 比而不是内容比：故意选便宜的做法，同内容的新字符串不会
    /// 触发刷新（这些字段本来就几乎不变）。
    /// </remarks>
    private static void UpdateUI()
    {
        if (!_running || _grid == null) return;

        // 防重入
        if (System.Threading.Interlocked.Exchange(ref _uiUpdating, 1) == 1) return;
        try
        {
            // 计算快照（含速率），沿用你现有统计
            List<(string key, ConnView view)> snapshot;
            lock (_lock)
            {
                snapshot = new List<(string, ConnView)>(_stats.Count);
                foreach (var kv in _stats)
                {
                    var ci = kv.Value;
                    long total = ci.BytesSent + ci.BytesRecv;
                    long delta = total - ci.LastTotal;
                    double seconds = Math.Max(1, _refreshMs) / 1000.0;
                    ci.SpeedBps = (long)(delta / seconds);
                    ci.LastTotal = total;

                    var view = new ConnView
                    {
                        ProcessName = ci.ProcessName,
                        PID = ci.PID,
                        FilePath = ci.FilePath,
                        Protocol = ci.Protocol,
                        Local = $"{ci.LocalIP}:{ci.LocalPort}",
                        Remote = $"{ci.RemoteIP}:{ci.RemotePort}",
                        BytesSent = ci.BytesSent,
                        BytesRecv = ci.BytesRecv,
                        SpeedBps = ci.SpeedBps
                    };
                    snapshot.Add((kv.Key, view));
                }
            }

            // UI 线程增量同步
            _grid.InvokeIfRequired(() =>
            {
                if (!_running || _grid.IsDisposed) return;

                _bs.SuspendBinding();
                _grid.SuspendLayout();

                // 标记现存项为“未见”，见到再打勾，最后统一移除未见项
                HashSet<string> seen = new();

                // 新增或更新
                foreach (var (key, view) in snapshot)
                {
                    seen.Add(key);
                    if (_viewIndex.TryGetValue(key, out var existing))
                    {
                        // 仅更新变化字段，避免不必要的 PropertyChanged 风暴
                        if (existing.BytesSent != view.BytesSent) existing.BytesSent = view.BytesSent;
                        if (existing.BytesRecv != view.BytesRecv) existing.BytesRecv = view.BytesRecv;
                        if (existing.SpeedBps != view.SpeedBps) existing.SpeedBps = view.SpeedBps;

                        // 进程名/路径/端口一般稳定，这里保持一致性（偶发变化也能同步）
                        if (!ReferenceEquals(existing.ProcessName, view.ProcessName)) existing.ProcessName = view.ProcessName;
                        if (existing.PID != view.PID) existing.PID = view.PID;
                        if (!ReferenceEquals(existing.FilePath, view.FilePath)) existing.FilePath = view.FilePath;
                        if (!ReferenceEquals(existing.Protocol, view.Protocol)) existing.Protocol = view.Protocol;
                        if (!ReferenceEquals(existing.Local, view.Local)) existing.Local = view.Local;
                        if (!ReferenceEquals(existing.Remote, view.Remote)) existing.Remote = view.Remote;
                    }
                    else
                    {
                        _binding.Add(view);
                        _viewIndex[key] = view;
                    }
                }

                // 删除消失的连接（倒序遍历更省心）
                for (int i = _binding.Count - 1; i >= 0; i--)
                {
                    var v = _binding[i];
                    // 找到其 key（用反查避免在 ConnView 里塞 key 字段）
                    // 由于 _viewIndex 是主索引，直接依据它来删即可
                }
                // 通过 _viewIndex 做真正的剔除
                var toRemove = _viewIndex.Keys.Where(k => !seen.Contains(k)).ToList();
                foreach (var k in toRemove)
                {
                    var v = _viewIndex[k];
                    _binding.Remove(v);
                    _viewIndex.Remove(k);
                }

                _grid.ResumeLayout(false);
                _bs.ResumeBinding();

                // 不调用 _grid.Refresh()，让 WinForms 自己安排重绘
            });

            // 写日志保持原逻辑
            AppendLog(snapshot.Select(s => s.view).ToList());
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref _uiUpdating, 0);
        }
    }

    // --- 日志 ---
    /// <summary>日志文件不存在就写表头（UTF-8，固定那 9 列，和 <see cref="AppendLog"/> 的列序一致）。失败只记日志，不能因为写不了 CSV 就不给看流量。</summary>
    private static void EnsureLogHeader()
    {
        try
        {
            if (!File.Exists(_csvLogPath))
            {
                File.WriteAllText(_csvLogPath, "Timestamp,PID,ProcessName,Protocol,Local,Remote,BytesSent,BytesRecv,SpeedBps\r\n", Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Npcap] 创建日志文件头失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 把这一轮的快照追写成 CSV（一行一个连接，时间用 <c>"o"</c> 格式便于 Excel/pandas 排序）。
    /// 注意这是「每轮全量追写」而不是「只写变化」，长时间开着文件会很快变大，而且它在刷新线程上同步写盘。
    /// </summary>
    private static void AppendLog(List<ConnView> rows)
    {
        if (rows.Count == 0) return;
        try
        {
            var sb = new StringBuilder();
            string now = DateTime.Now.ToString("o");
            foreach (var r in rows)
            {
                sb.AppendLine($"{now},{r.PID},{EscapeCsv(r.ProcessName)},{r.Protocol},{EscapeCsv(r.Local)},{EscapeCsv(r.Remote)},{r.BytesSent},{r.BytesRecv},{r.SpeedBps}");
            }
            File.AppendAllText(_csvLogPath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Npcap] 写入日志失败: {ex.Message}");
        }
    }

    /// <summary>CSV 字段转义：带逗号/引号/换行就用双引号包起来，里面的引号翻成两个。进程名允许带逗号，不转义会错列。</summary>
    private static string EscapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            return $"\"{s.Replace("\"", "\"\"")}\"";
        return s;
    }

    // --- 辅助：获取进程名与路径（带缓存）---
    /// <summary>
    /// 按 PID 取进程名与主模块路径，结果进 <see cref="_processCache"/>。
    /// 常见情况：进程已退出或目标是别人的高权限进程 —— 拿不到路径就留空串，名字退回 <c>"N/A"</c>，
    /// 不抛异常（表格里那一行只是少列信息，不应该影响抓包）。
    /// </summary>
    private static (string name, string path) GetProcessNameAndPath(int pid)
    {
        try
        {
            // 这个方法在抓包回调里按连接调，不 Dispose 等于每个连接漏一个内核句柄
            using var p = Process.GetProcessById(pid);
            string name = p.ProcessName;
            string path = "";
            try
            {
                path = p.MainModule?.FileName ?? "";
            }
            catch (Win32Exception) { path = ""; } // 可能无权限
            catch (InvalidOperationException) { path = ""; } // 进程已退出
            return (name, path);
        }
        catch
        {
            return ("N/A", "");
        }
    }


    /// <summary>
    /// 把一个包归到进程：拿系统连接表（TCP 用 <c>GetAllTcpConnections</c>，UDP 用
    /// <c>GetAllUdpListeners</c>）比对本地地址+端口，命中哪个端就是哪个方向。
    /// </summary>
    /// <param name="proto"><c>"TCP"</c> 或 <c>"UDP"</c>，决定查哪张系统表。</param>
    /// <param name="srcIP">包里源地址（去掉了方括号，字面要跟系统表一致才比得中）。</param>
    /// <param name="srcPort">源端口。</param>
    /// <param name="dstIP">包里目的地址。</param>
    /// <param name="dstPort">目的端口。</param>
    /// <param name="isOutbound">输出：本地端命中了 <c>srcIP:srcPort</c> 就是出站（true），
    /// 命中 <c>dstIP:dstPort</c> 就是入站。调用方靠它决定哪个地址算「本地」、字节算收还是算发。</param>
    /// <returns>匹配到的 PID；查不到返回 <c>-1</c>（调用方直接丢包）。</returns>
    /// <remarks>
    /// 两个已知代价，要改之前先看这里：
    /// 一是它在 <see cref="Device_OnPacketArrival"/> 里被「每个包」调一次，每次都重新抓一份全量连接表
    /// 再线性比，量级是「包数 × 连接数」，流量一大抓包线程就被它拖死（下面那些按连接写的
    /// <c>Debug.WriteLine</c> 又把这笔开销放大一轮）——要做成定时刷新缓存 + 字典查；
    /// 二是只能查到「现在还在表里」的连接：短连接包到得比系统表更新快、或连接已断时，会归不到进程。
    /// </remarks>
    private static int PortToPID(string proto, string srcIP, int srcPort, string dstIP, int dstPort, out bool isOutbound)
    {
        isOutbound = true;
        Debug.WriteLine($"[PortToPID] 查询: {proto} {srcIP}:{srcPort} -> {dstIP}:{dstPort}");

        try
        {
            if (proto == "TCP")
            {
                Debug.WriteLine($"[PortToPID] 尝试匹配 TCP 连接...");

                var list = IPHelper.GetAllTcpConnections();


                Debug.WriteLine($"[Debug] GetAllTcpConnections() 返回的 IEnumerable 对象: {list}"); // 这会显示类型

                int count = 0;
                try
                {
                    foreach (var item in list)
                    {
                        count++;
                        Debug.WriteLine($"[Debug] TCP 连接 {count}: PID={item.ProcessId}, " +
                                        $"本地={item.LocalAddress}:{item.LocalPort}, " +
                                        $"远程={item.RemoteAddress}:{item.RemotePort}");
                    }
                    Debug.WriteLine($"[Debug] 总共找到 {count} 个 TCP 连接。");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Debug] 在遍历 GetAllTcpConnections() 结果时发生异常: {ex}");
                }

                foreach (var r in list)
                {
                    Debug.WriteLine($"  [TCP] 检查: {r.LocalAddress}:{r.LocalPort} <-> {r.RemoteAddress}:{r.RemotePort} (PID={r.ProcessId})");
                    if (r.LocalAddress == srcIP && r.LocalPort == srcPort)
                    {
                        Debug.WriteLine($"[PortToPID] 匹配出站 TCP，PID={r.ProcessId}");
                        isOutbound = true;
                        return r.ProcessId;
                    }
                    if (r.LocalAddress == dstIP && r.LocalPort == dstPort)
                    {
                        Debug.WriteLine($"[PortToPID] 匹配入站 TCP，PID={r.ProcessId}");
                        isOutbound = false;
                        return r.ProcessId;
                    }
                }
                Debug.WriteLine("[PortToPID] TCP 查询结束，未找到匹配");
            }
            else if (proto == "UDP")
            {
                Debug.WriteLine($"[PortToPID] 尝试匹配 UDP 监听...");
                foreach (var r in IPHelper.GetAllUdpListeners())
                {
                    Debug.WriteLine($"  [UDP] 检查: {r.LocalAddress}:{r.LocalPort} (PID={r.ProcessId})");
                    if (r.LocalAddress == srcIP && r.LocalPort == srcPort)
                    {
                        Debug.WriteLine($"[PortToPID] 匹配出站 UDP，PID={r.ProcessId}");
                        isOutbound = true;
                        return r.ProcessId;
                    }
                    if (r.LocalAddress == dstIP && r.LocalPort == dstPort)
                    {
                        Debug.WriteLine($"[PortToPID] 匹配入站 UDP，PID={r.ProcessId}");
                        isOutbound = false;
                        return r.ProcessId;
                    }
                }
                Debug.WriteLine("[PortToPID] UDP 查询结束，未找到匹配");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Npcap] 查询端口映射失败: {ex}");
        }

        Debug.WriteLine("[PortToPID] 返回 -1 (无法归属)");
        return -1;
    }

  
}