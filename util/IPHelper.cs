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
    public static class IPHelper
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, int ulAf, TCP_TABLE_CLASS TableClass, uint Reserved = 0);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, bool bOrder, int ulAf, UDP_TABLE_CLASS TableClass, uint Reserved = 0);

        enum TCP_TABLE_CLASS { TCP_TABLE_OWNER_PID_ALL = 5 }
        enum UDP_TABLE_CLASS { UDP_TABLE_OWNER_PID = 1 }

        // ======== 修正后的结构体：使用 ushort ========
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public ushort localPort;   // 从 uint 改为 ushort
            public uint remoteAddr;
            public ushort remotePort;  // 从 uint 改为 ushort
            public uint owningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MIB_UDPROW_OWNER_PID
        {
            public uint localAddr;
            public ushort localPort;   // 从 uint 改为 ushort
            public uint owningPid;
        }
        // ======== 修正后的结构体：使用 ushort ========

        public struct TcpRowEx
        {
            public string LocalAddress;
            public int LocalPort;
            public string RemoteAddress;
            public int RemotePort;
            public int ProcessId;
        }

        public struct UdpRowEx
        {
            public string LocalAddress;
            public int LocalPort;
            public int ProcessId;
        }

        private const int AF_INET = 2;                 // IPv4
        private const uint NO_ERROR = 0;               // 成功
        private const uint ERROR_INSUFFICIENT_BUFFER = 122; // 缓冲区不足


        // ======== 修正后的 GetAllTcpConnections ========

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
