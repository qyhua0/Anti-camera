/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：util/IPHelper.cs
 * 作者：zhenhua
 * 说明：用 iphlpapi 的 GetExtendedTcpTable / GetExtendedUdpTable 拿「哪个进程占着哪条连接」，
 * 返回带 PID 的 TCP/UDP 行，供网络模块把连接归到进程头上（无需抓包，成本极低）。
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDisplay.util
{
    /// <summary>
    /// 网络信息工具类：调用 iphlpapi 的扩展表接口，枚举本机连接（TCP）与监听端口（UDP），
    /// 并带出每条连接/每个监听端口所属的进程 PID，供网络模块把连接归到进程头上。
    /// 全程只读、无副作用；所有失败路径统一退化为「写 Debug 日志 + 返回空集合」，调用方不需要 try/catch。
    /// </summary>
    public static class IPHelper
    {
        /// <summary>
        /// P/Invoke iphlpapi.dll 的 GetExtendedTcpTable：一次调用取回本机完整的 TCP 连接表（含拥有者 PID），无需抓包。
        /// </summary>
        /// <param name="pTcpTable">表缓冲区指针。传 <see cref="IntPtr.Zero"/> 表示只做大小探测，此时 API 把所需字节数写回 <paramref name="pdwSize"/>。</param>
        /// <param name="pdwSize">输入/输出参数：缓冲区字节数；缓冲区不足时由 API 改写为实际所需大小。</param>
        /// <param name="bOrder">true 表示按本地地址/端口排序返回，结果顺序稳定。</param>
        /// <param name="ulAf">地址族（AF）：2 = AF_INET（IPv4），23 = AF_INET6。本文件只用 IPv4。</param>
        /// <param name="TableClass">表类型，取 TCP_TABLE_OWNER_PID_ALL(5)：全部 TCP 行 + 拥有者 PID。</param>
        /// <param name="Reserved">保留字段，必须传 0。</param>
        /// <returns>0（NO_ERROR）表示成功；122（ERROR_INSUFFICIENT_BUFFER）表示缓冲区不足、需按新的 pdwSize 重试；其余为 Win32 错误码。</returns>
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, int ulAf, TCP_TABLE_CLASS TableClass, uint Reserved = 0);

        /// <summary>
        /// P/Invoke iphlpapi.dll 的 GetExtendedUdpTable：取回本机 UDP 监听端口表（含拥有者 PID）。
        /// 调用方式与 <see cref="GetExtendedTcpTable"/> 相同：先传空指针探大小，再分配缓冲区取数。
        /// </summary>
        /// <param name="pUdpTable">UDP 表缓冲区指针；传 <see cref="IntPtr.Zero"/> 只做大小探测。</param>
        /// <param name="pdwSize">输入/输出参数：缓冲区字节数，不足时被改写为所需大小。</param>
        /// <param name="bOrder">true 表示排序返回。</param>
        /// <param name="ulAf">地址族：2 = AF_INET（IPv4）。</param>
        /// <param name="TableClass">表类型，取 UDP_TABLE_OWNER_PID(1)：UDP 监听行 + 拥有者 PID。</param>
        /// <param name="Reserved">保留字段，必须传 0。</param>
        /// <returns>0 成功；122 缓冲区不足；其余为 Win32 错误码。</returns>
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, bool bOrder, int ulAf, UDP_TABLE_CLASS TableClass, uint Reserved = 0);

        /// <summary>MIB_TCP_TABLE_CLASS 的本地精简副本，本文件只需要「全部连接 + 拥有者 PID」这一种表。</summary>
        enum TCP_TABLE_CLASS { TCP_TABLE_OWNER_PID_ALL = 5 }

        /// <summary>MIB_UDP_TABLE_CLASS 的本地精简副本，本文件只需要「监听端口 + 拥有者 PID」这一种表。</summary>
        enum UDP_TABLE_CLASS { UDP_TABLE_OWNER_PID = 1 }

        // ======== 修正后的结构体：使用 ushort ========
        /// <summary>
        /// 原生 MIB_TCPROW_OWNER_PID 的托管映射：GetExtendedTcpTable 表体中的一条 TCP 连接（IPv4）。
        /// 原生把端口放在 4 字节 DWORD 的低 16 位（高 16 位为 0），这里用 ushort 只取低 16 位；
        /// 顺序布局下自动补齐后各字段偏移与原生完全一致，结构体总长仍为 24 字节，可直接按 24 字节步进遍历。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_TCPROW_OWNER_PID
        {
            public uint state;         // TCP 连接状态码（MIB_TCP_STATE）：1=CLOSED 2=LISTEN（本机监听端口）3=SYN_SENT 4=SYN_RCVD 5=ESTABLISHED（已建立连接）6=FIN_WAIT1 7=FIN_WAIT2 8=CLOSE_WAIT 9=CLOSING 10=TIME_WAIT
            public uint localAddr;     // 本机 IPv4 地址，网络字节序压在 4 字节里（故用 BitConverter.GetBytes 按字节还原成点分十进制）
            public ushort localPort;   // 本机端口，网络字节序，需 NetworkToHostOrder 转主机序；从 uint 改为 ushort
            public uint remoteAddr;    // 远端 IPv4 地址，网络字节序；未连接（LISTEN）时为 0.0.0.0
            public ushort remotePort;  // 远端端口，网络字节序；从 uint 改为 ushort
            public uint owningPid;     // 拥有该连接的进程 PID（原生 DWORD，交给上层时 unchecked 转 int）
        }

        /// <summary>
        /// 原生 MIB_UDPROW_OWNER_PID 的托管映射：GetExtendedUdpTable 表体中的一条 UDP 监听项（IPv4）。
        /// UDP 无连接，所以只有本地地址/本地端口/拥有者 PID 三项；端口同样取原生 DWORD 的低 16 位，
        /// 补齐后结构体总长 12 字节，与原生行宽一致。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_UDPROW_OWNER_PID
        {
            public uint localAddr;     // 本机 IPv4 地址，网络字节序；0.0.0.0 表示监听所有网卡
            public ushort localPort;   // 本机监听端口，网络字节序；从 uint 改为 ushort
            public uint owningPid;     // 占用该监听端口的进程 PID
        }
        // ======== 修正后的结构体：使用 ushort ========

        /// <summary>对外的「一条本机连接」视图，由原生 TCP 行转换而来（数值字段均已转成主机字节序）。</summary>
        public struct TcpRowEx
        {
            public string LocalAddress;   // 本机 IPv4 地址，点分十进制字符串
            public int LocalPort;         // 本机端口，主机字节序的十进制数值
            public string RemoteAddress;  // 远端 IPv4 地址，点分十进制字符串；监听行为 0.0.0.0
            public int RemotePort;        // 远端端口，主机字节序的十进制数值
            public int ProcessId;         // 拥有该连接的进程 PID，可用于查进程名
        }

        /// <summary>对外的「一个本机监听端口（UDP）」视图，由原生 UDP 行转换而来。</summary>
        public struct UdpRowEx
        {
            public string LocalAddress;   // 本机 IPv4 地址，点分十进制字符串；0.0.0.0 表示监听所有网卡
            public int LocalPort;         // 监听端口，主机字节序的十进制数值
            public int ProcessId;         // 占用该端口的进程 PID
        }

        /// <summary>地址族常量 AF_INET：只查 IPv4 表（IPv6 表需要另传 AF_INET6=23，本工具未使用）。</summary>
        private const int AF_INET = 2;                 // IPv4
        /// <summary>Win32 返回值：操作成功。</summary>
        private const uint NO_ERROR = 0;               // 成功
        /// <summary>Win32 返回值 ERROR_INSUFFICIENT_BUFFER：缓冲区不够大，属预期结果，需按 API 回写的字节数重试。</summary>
        private const uint ERROR_INSUFFICIENT_BUFFER = 122; // 缓冲区不足


        // ======== 修正后的 GetAllTcpConnections ========

        /// <summary>
        /// 枚举本机全部 TCP 连接与监听端口（IPv4）：先用空缓冲区探测所需大小，再分配非托管内存取表，
        /// 跳过表头 4 字节 dwNumEntries 后按每行 24 字节逐条解析。
        /// 若表项在本次调用期间增多（API 再次回写更大的 buffSize），带着新大小重试，最多 3 次。
        /// </summary>
        /// <returns>惰性序列，每个元素一条本机连接：地址为点分十进制 IPv4，端口已转为主机字节序。
        /// 探测失败、重试 3 次仍缓冲区不足、或原生返回任何非 0 错误码时，只写 Debug 日志并结束迭代，即返回空集合，不抛异常。</returns>
        public static IEnumerable<TcpRowEx> GetAllTcpConnections()
        {
            int buffSize = 0;

            // 第一次：探测大小（122 是预期结果）
            uint ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (ret != ERROR_INSUFFICIENT_BUFFER && ret != NO_ERROR)
            {
                System.Diagnostics.Debug.WriteLine($"[IPHelper] probe failed: {ret}, {new Win32Exception((int)ret).Message}");
                yield break;
            }

            // 分配缓冲区并真正取数。注意：buffSize 可能在第二次调用中再次被增大，所以做一层循环以应对竞态增长
            IntPtr buff = IntPtr.Zero;
            try
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    if (buff != IntPtr.Zero) Marshal.FreeHGlobal(buff);
                    buff = Marshal.AllocHGlobal(buffSize);

                    ret = GetExtendedTcpTable(buff, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                    if (ret == NO_ERROR) break;
                    if (ret != ERROR_INSUFFICIENT_BUFFER)
                    {
                        System.Diagnostics.Debug.WriteLine($"[IPHelper] data call failed: {ret}, {new Win32Exception((int)ret).Message}");
                        yield break;
                    }
                    // 表项数在调用期间增多了 -> buffSize 被更新，重试
                    System.Diagnostics.Debug.WriteLine($"[IPHelper] buffer grew, retrying with {buffSize} bytes... ({attempt + 1}/3)");
                }

                if (ret != NO_ERROR)
                {
                    System.Diagnostics.Debug.WriteLine($"[IPHelper] failed after retries: {ret}");
                    yield break;
                }

                // 读取条目数量
                int numEntries = Marshal.ReadInt32(buff);
                System.Diagnostics.Debug.WriteLine($"[IPHelper] Found {numEntries} TCP entries");

                IntPtr rowPtr = IntPtr.Add(buff, sizeof(int)); // 跳过 dwNumEntries
                int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>(); // 24 字节
                for (int i = 0; i < numEntries; i++)
                {
                    MIB_TCPROW_OWNER_PID row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);

                    // IP 地址（按字节构造，避免端序混淆）
                    string localAddrStr = new IPAddress(BitConverter.GetBytes(row.localAddr)).ToString();
                    string remoteAddrStr = new IPAddress(BitConverter.GetBytes(row.remoteAddr)).ToString();

                    // 端口：字段是网络序的低16位
                    int localPort = (ushort)IPAddress.NetworkToHostOrder((short)(row.localPort & 0xFFFF));
                    int remotePort = (ushort)IPAddress.NetworkToHostOrder((short)(row.remotePort & 0xFFFF));

                    yield return new TcpRowEx
                    {
                        LocalAddress = localAddrStr,
                        LocalPort = localPort,
                        RemoteAddress = remoteAddrStr,
                        RemotePort = remotePort,
                        ProcessId = unchecked((int)row.owningPid)
                    };

                    rowPtr = IntPtr.Add(rowPtr, rowSize);
                }
            }
            finally
            {
                if (buff != IntPtr.Zero) Marshal.FreeHGlobal(buff);
            }
        }





        // ======== 修正后的 GetAllTcpConnections ========

        // ======== 为 GetAllUdpListeners() 应用同样的修复 ========
        /// <summary>
        /// 枚举本机 UDP 监听端口（IPv4）及各自所属进程，供「监听端口」清单使用。
        /// 先用空缓冲区探测大小（非 0 且非 122 视为失败；否则歇 10ms 重试，最多 3 次），再取表逐行解析；
        /// 单行解析异常只记日志并跳过该行，不影响其余行。与 TCP 版本不同：结果先整体收进 List，解析完再一次性产出。
        /// </summary>
        /// <returns>惰性序列，每个元素一个 UDP 监听项：地址解析失败时按 0.0.0.0 给出，端口已转为主机字节序。
        /// 大小探测最终失败、取表失败或发生未预期异常时，只写 Debug 日志并返回空集合，不抛异常。非托管缓冲区在 finally 中必定释放。</returns>
        public static IEnumerable<UdpRowEx> GetAllUdpListeners()
        {
            int AF_INET = 2; // IPv4
            int buffSize = 0;
            uint ret = 0;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                ret = GetExtendedUdpTable(IntPtr.Zero, ref buffSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
                if (ret == 0) break;
                if (ret != 122)
                {
                    Debug.WriteLine($"[IPHelper] GetExtendedUdpTable (size) failed (attempt {attempt + 1}): {ret}");
                    yield break;
                }
                Thread.Sleep(10);
            }

            if (ret != 0)
            {
                Debug.WriteLine($"[IPHelper] GetExtendedUdpTable (size) failed after retries: {ret}");
                yield break;
            }

            IntPtr buff = Marshal.AllocHGlobal(buffSize);
            List<UdpRowEx> results = new List<UdpRowEx>();
            bool success = false;

            try
            {
                ret = GetExtendedUdpTable(buff, ref buffSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID);
                if (ret != 0)
                {
                    Debug.WriteLine($"[IPHelper] GetExtendedUdpTable (data) failed: {ret}");
                    yield break;
                }

                uint numEntries = (uint)Marshal.ReadInt32(buff);
                Debug.WriteLine($"[IPHelper] Found {numEntries} UDP listeners");
                IntPtr rowPtr = IntPtr.Add(buff, 4);
                int rowSize = Marshal.SizeOf(typeof(MIB_UDPROW_OWNER_PID));

                for (int i = 0; i < numEntries; i++)
                {
                    try
                    {
                        var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);
                        Debug.WriteLine($"  [Raw UDP Row {i}] localAddr={row.localAddr}, localPort={row.localPort}, owningPid={row.owningPid}");

                        string localAddrStr;
                        try
                        {
                            localAddrStr = new IPAddress(BitConverter.GetBytes(row.localAddr)).ToString();
                        }
                        catch
                        {
                            localAddrStr = "0.0.0.0";
                        }

                        int localPort = (int)IPAddress.NetworkToHostOrder((short)row.localPort);
                        int pid = (int)row.owningPid;

                        results.Add(new UdpRowEx
                        {
                            LocalAddress = localAddrStr,
                            LocalPort = localPort,
                            ProcessId = pid
                        });

                        Debug.WriteLine($"  [Parsed UDP] {localAddrStr}:{localPort} (PID={pid})");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[IPHelper] Failed to process UDP row {i}: {ex.Message}");
                    }
                    finally
                    {
                        rowPtr = IntPtr.Add(rowPtr, rowSize);
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IPHelper] Critical error in GetAllUdpListeners: {ex}");
            }
            finally
            {
                if (buff != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buff);
                    Debug.WriteLine($"[IPHelper] UDP Memory buffer freed, success={success}");
                }
            }

            foreach (var item in results)
            {
                yield return item;
            }
        }
        // ======== 为 GetAllUdpListeners() 应用同样的修复 ========
    }
}
