using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using SharpPcap;
using PacketDotNet;
using WinDisplay.util;


/// <summary>
/// 使用 Npcap 监控网络流量，按进程统计连接和带宽。
/// 必须以管理员权限运行，并安装 Npcap。
/// </summary>
public static class NpcapNetworkMonitor

{

    // ===== 新增字段 =====
    private static BindingList<ConnView> _binding;
    private static BindingSource _bs;
    private static readonly Dictionary<string, ConnView> _viewIndex = new(); // key -> ConnView
    private static int _uiUpdating = 0; // 0: idle, 1: updating

    // ===== 工具：给 DGV 开启双缓冲（WinForms 反射）=====
    private static void EnableDoubleBuffer(DataGridView dgv)
    {
        typeof(DataGridView).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
            null, dgv, new object[] { true });
    }



    // --- 公共 Start/Stop 接口 ---
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

            // 列宽只在第一次初始化时给个基础值（存在就不覆盖）
            foreach (DataGridViewColumn col in _grid.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                if (col.Width < 60) col.Width = 90;
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

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
        if (_grid != null)
        {
            _grid.InvokeIfRequired(() =>
            {
                if (!_grid.IsDisposed)
                {
                    // 不重建 DataSource，只清空数据，保留列与列宽
                    _bs?.SuspendBinding();
                    _binding?.Clear();
                    _viewIndex.Clear();
                    _bs?.ResumeBinding();
                    // 不调用 _grid.Refresh(); 留给 WinForms 自己重绘
                }
            });
        }

        Debug.WriteLine("[Npcap] 监控已停止");
    }

    // --- 内部数据结构 ---
    private class ConnInfo
    {
        public int PID;
        public string ProcessName;
        public string FilePath;
        public string Protocol; // "TCP"/"UDP"
        public string LocalIP;
        public int LocalPort;
        public string RemoteIP;
        public int RemotePort;
        public long BytesSent;
        public long BytesRecv;
        public long LastTotal; // 上次累计的总字节数，用于计算速率
        public long SpeedBps;  // 当前速率 (Bytes per second)
    }

    public class ConnView
    {
        public string ProcessName { get; set; }
        public int PID { get; set; }
        public string FilePath { get; set; }
        public string Protocol { get; set; }
        public string Local { get; set; }
        public string Remote { get; set; }
        public long BytesSent { get; set; }
        public long BytesRecv { get; set; }
        public long SpeedBps { get; set; }
    }

    // --- 字段 ---
    private static ICaptureDevice _device;
    private static DataGridView _grid;
    private static System.Threading.Timer _uiTimer;
    private static readonly object _lock = new object();
    private static readonly Dictionary<string, ConnInfo> _stats = new Dictionary<string, ConnInfo>();
    private static readonly Dictionary<int, (string name, string path)> _processCache = new Dictionary<int, (string, string)>();
    private static bool _running = false;
    private static int _refreshMs = 1000;
    private static string _csvLogPath = "net_npcap_log.csv";

    // --- 事件回调 ---
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


            // 生成唯一键 (包含方向)
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

    private static string EscapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            return $"\"{s.Replace("\"", "\"\"")}\"";
        return s;
    }

    // --- 辅助：获取进程名与路径（带缓存）---
    private static (string name, string path) GetProcessNameAndPath(int pid)
    {
        try
        {
            var p = Process.GetProcessById(pid);
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


    // --- 端口 -> PID 映射 ---
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