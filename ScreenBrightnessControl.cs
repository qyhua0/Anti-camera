/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：ScreenBrightnessControl.cs
 * 作者：zhenhua
 * 说明：用显卡伽马曲线（LUT）压暗屏幕：SetDeviceGammaRamp。不是改显示器亮度寄存器，所以外接屏也吃这套。
 * 实测驱动只允许 LUT 偏离恒等曲线约 ≤32769（约 50%），因此本类会二分探测本机真实下限并缓存，
 * 连恒等曲线都设不进去时（远程桌面/虚拟显卡）判定为不支持，让界面把功能置灰而不是每次报错。
 */
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinDisplay
{
    /// <summary>
    /// 用显卡伽马曲线（LUT）给屏幕「调暗」，不是改显示器亮度寄存器，所以外接显示器也吃这套。
    /// <para>
    /// <b>重要限制（实测，不是猜的）</b>：显卡驱动对 LUT 相对恒等曲线的偏离量有上限。
    /// 在 Intel Iris Xe + Windows 11 上实测：任意通道任意一个采样点偏离恒等值 ≤ 32769 时被接受，
    /// 到 33000 就被拒绝，而 <c>SetDeviceGammaRamp</c> 返回 FALSE 时 <b>不设置 LastError</b>（拿到的是残留值）。
    /// 直接按比例压暗（白色点 = 亮度%）等价于偏离 (1-亮度)，所以<b>低于约 50% 全部会被驱动拒绝</b>。
    /// 旧实现既没检查返回值也没探测下限，于是「拖到 50% 以下其实一点没变暗」这个bug一直存在。
    /// </para>
    /// <para>
    /// 本类因此做三件事：① 首次压到某个值失败时二分探测本机真正的下限并缓存；
    /// ② 之后低于该下限直接夹住，只回一句提示，不再反复试（避免拖动时反复闪屏）；
    /// ③ 连 100%（恒等曲线）都设不进去时，判定设备不支持（远程桌面、部分虚拟显卡），
    ///    之后不再尝试，让界面可以把这个功能置灰而不是每次都报错。
    /// </para>
    /// <para>
    /// 另外两点：修改的是显示设备状态，进程退出时 Windows 会自动还原，但正常关闭仍应主动
    /// <see cref="ResetBrightness()"/>；且只作用于主显示设备（GetDC(IntPtr.Zero)），
    /// 与遮罩窗口当前的多屏行为一致。
    /// </para>
    /// <para>
    /// <b>两级减光</b>：伽马担不到的那段由 <see cref="ScreenDimOverlay"/>（全屏半透明遮罩）接着压，
    /// 总亮度 = 伽马亮度 × 遮罩透光率，拼装在 <c>frm_screen.ApplyBrightness</c>。
    /// 因此本类被下限夹住不算“失败”，不该弹给用户；本机实测能拉到 5%（= 伽马 50% × 遮罩 10%），
    /// 再往下的请求会被夹到 5%（遮罩透光率最低 10%，留一层“至少还能看见”的安全垫）。
    /// </para>
    /// </summary>
    internal class ScreenBrightnessControl
    {
        /// <summary>亮度上限，100% 即恒等曲线（完全不改变显示）。</summary>
        public const int MaxBrightness = 100;

        // Windows API 声明
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP ramp);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumDisplayDevices(string? deviceName, int deviceNum, IntPtr displayDevice, int flags);

        private const int SM_REMOTESESSION = 0x1000;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue;
        }

        private static readonly object Gate = new object();

        private static bool? _deviceSupportsGamma;   // null = 还没试过
        private static int _floorPercent = -1;       // 实测能设进去的最小亮度，-1 = 未知
        private static string? _deviceName;

        /// <summary>当前是否远程桌面会话（这种会话里伽马曲线一律改不动）。</summary>
        public static bool IsRemoteSession => GetSystemMetrics(SM_REMOTESESSION) != 0;

        /// <summary>设备是否支持伽马调暗；null 表示还没试过。</summary>
        public static bool? DeviceSupportsGamma => _deviceSupportsGamma;

        /// <summary>本机实测能设进去的最小亮度百分比，-1 表示还没探测过。</summary>
        public static int FloorPercent => _floorPercent;

        /// <summary>主显示设备名，用于把报错写得能定位（例如 Intel(R) Iris(R) Xe Graphics）。</summary>
        public static string DeviceName => _deviceName ??= QueryPrimaryDeviceName();

        /// <summary>
        /// 设置屏幕亮度（0-100，100 为原始亮度）。
        /// </summary>
        /// <exception cref="InvalidOperationException">设备不支持，或取不到屏幕 DC</exception>
        public static void SetBrightness(int brightness)
        {
            // 被显卡上限夹住不算失败（已经设到了能设的最暗值），所以这里不看 message
            if (!TrySetBrightness(brightness, out string? message, out _))
                throw new InvalidOperationException(message);
        }

        /// <summary>设置屏幕亮度，失败只返回 false 不抛异常（供关闭流程、托盘菜单等路径使用）。</summary>
        public static bool TrySetBrightness(int brightness, out string? error)
            => TrySetBrightness(brightness, out error, out _);

        /// <summary>
        /// 设置亮度并告诉调用方「实际设到了多少」。
        /// </summary>
        /// <param name="brightness">请求的亮度 0-100</param>
        /// <param name="message">需要转告用户的话（被显卡上限夹住时的说明，或失败原因）。成功且没被夹住时为 null</param>
        /// <param name="applied">实际生效的亮度；失败时为 -1</param>
        /// <returns>是否至少设到了一个可用值（被夹住也算 true）</returns>
        public static bool TrySetBrightness(int brightness, out string? message, out int applied)
        {
            message = null;
            applied = -1;
            int requested = brightness;

            if (brightness < 0 || brightness > MaxBrightness)
            {
                message = $"亮度必须在 0-{MaxBrightness} 之间，收到 {brightness}";
                return false;
            }

            if (IsRemoteSession)
            {
                _deviceSupportsGamma = false;
                message = "当前是远程桌面会话，Windows 不允许它改显卡伽马曲线，亮度调节在这个环境下无效。";
                return false;
            }

            lock (Gate)
            {
                // 下限已知就不要再试探：拖动滑块时每个像素都进来一次，反复失败会闪屏
                if (_floorPercent > 0 && brightness < _floorPercent) brightness = _floorPercent;

                IntPtr hdc = GetDC(IntPtr.Zero);
                if (hdc == IntPtr.Zero)
                {
                    message = $"无法获取屏幕设备上下文（GetDC 失败，错误码 {Marshal.GetLastWin32Error()}）";
                    return false;
                }

                try
                {
                    if (TryApply(hdc, brightness, 1.0, out _))
                    {
                        _deviceSupportsGamma = true;
                        // 下限只能由「失败 + 二分探测」得出，成功绝不能初始化它：
                        // 否则第一次成功设 100% 就把下限写成 100，之后任何变暗请求都会被夹回 100，功能等于废掉。
                        // 这里只允许「成功值低于已知下限」时把下限放宽（驱动状态变了）。
                        if (_floorPercent > 0 && brightness < _floorPercent) _floorPercent = brightness;
                        applied = brightness;
                        message = ClampNote(requested, brightness);
                        return true;
                    }

                    // 失败。先分清是「设备根本不支持」还是「只是压得太低」：恒等曲线能不能设进去
                    if (!TryApply(hdc, MaxBrightness, 1.0, out int identityErr))
                    {
                        _deviceSupportsGamma = false;
                        applied = -1;
                        message = $"本机显示设备「{DeviceName}」不接受设置伽马曲线（SetDeviceGammaRamp 返回失败，" +
                                  $"系统错误码 {identityErr}——该 API 失败时常常不填错误码）。" +
                                  "远程桌面、部分虚拟显卡和精简显卡驱动就是这样。已改由半透明遮罩（ScreenDimOverlay）承担减光，" +
                                  "功能不会失效，只是靠窗口叠加而不是降背光。";
                        return false;
                    }

                    _deviceSupportsGamma = true;

                    // 设备支持，只是压过头了 —— 二分找出本机最暗可用值（此时 100 已确认可用）
                    int lo = brightness, hi = MaxBrightness;
                    while (hi - lo > 1)
                    {
                        int mid = (lo + hi) / 2;
                        if (TryApply(hdc, mid, 1.0, out _)) hi = mid;
                        else lo = mid;
                    }
                    TryApply(hdc, hi, 1.0, out _);      // 保证停在可用值上

                    if (hi <= brightness + 1)
                    {
                        // 二分结果说明「你要的值其实能设进去」，即刚才那次失败是偶发的
                        // （切分辨率/外接屏、驱动瞬时 busy）。这种情况下限不能缓存，
                        // 否则一次抖动能把亮度永久卡在错误值上。按请求值再试一次就收尾。
                        applied = TryApply(hdc, brightness, 1.0, out _) ? brightness : hi;
                        return true;
                    }

                    _floorPercent = hi;
                    applied = hi;
                    // 这不是“失败了”，两级减光的正常分工：伽马担到下限，剩下的差额由遮罩接着压，
                    // 所以调用方（frm_screen.ApplyBrightness）不需要把这句话弹给用户
                    message = $"显卡驱动不接受 {requested}%：伽马曲线最低只能到 {hi}%（LUT 相对原亮度的偏离量有上限，实测约一半），" +
                              "差额已改由半透明遮罩承担。";
                    return true;
                }
                finally
                {
                    // 原实现每次调用都 GetDC 却从不 ReleaseDC（连声明都没有）：稳定的 GDI 句柄泄漏，
                    // 拖几下亮度条就朝进程默认的 10000 个 GDI 对象上限逼近
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }
        }

        /// <summary>恢复原始亮度。失败只返回 false。</summary>
        public static bool ResetBrightness(out string? error) => TrySetBrightness(MaxBrightness, out error);

        /// <summary>恢复原始亮度（出错抛异常）。</summary>
        public static void ResetBrightness() => SetBrightness(MaxBrightness);

        /// <summary>
        /// 兼容旧调用点。
        /// <para>
        /// 以前它内部是 exponent=3 的抬升曲线，而按钮走的 <see cref="SetBrightness"/> 是线性曲线，
        /// 同一个数值两种方式算出来的亮度不一样，点按钮和拖滑块之间屏幕会跳。现在默认 exponent=1。
        /// </para>
        /// </summary>
        public static void SetBrightnessAPI(int brightness, double exponent = 1.0)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                throw new InvalidOperationException($"无法获取屏幕设备上下文（GetDC 失败，错误码 {Marshal.GetLastWin32Error()}）");
            try
            {
                if (!TryApply(hdc, brightness, exponent, out _))
                    throw new InvalidOperationException(
                        $"显卡驱动不接受 exponent={exponent:0.##} 的曲线（brightness={brightness}）。" +
                        "伽马曲线每一点相对原亮度的偏离不能超过一半左右。");
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        /// <summary>被显卡上限夹住时给用户的一句话（没被夹住返回 null）。</summary>
        private static string? ClampNote(int requested, int applied)
            => applied == requested
                ? null
                : $"显卡驱动最低只接受 {applied}%，已按 {applied}% 设置（请求的是 {requested}%）。";

        /// <summary>
        /// 只负责「生成曲线 + 送进驱动」，不碰 DC。
        /// 错误码必须紧跟失败的调用读，后面再调一次别的 SetLastError 的 API 就会被刷掉。
        /// </summary>
        private static bool TryApply(IntPtr hdc, int brightness, double exponent, out int lastError)
        {
            RAMP ramp = BuildRamp(brightness, exponent);
            bool ok = SetDeviceGammaRamp(hdc, ref ramp);
            lastError = Marshal.GetLastWin32Error();
            return ok;
        }

        /// <summary>
        /// 生成三通道一致的伽马曲线。exponent=1、brightness=100 时得到恒等曲线
        /// （ramp[i] = i * 257，最大值正好 65535）。
        /// </summary>
        private static RAMP BuildRamp(int brightness, double exponent)
        {
            if (exponent <= 0) exponent = 1.0;
            double factor = brightness / (double)MaxBrightness;

            RAMP ramp = new RAMP
            {
                Red = new ushort[256],
                Green = new ushort[256],
                Blue = new ushort[256]
            };

            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double shaped = exponent == 1.0 ? normalized : Math.Pow(normalized, 1.0 / exponent);
                if (shaped > 1.0) shaped = 1.0;

                int value = (int)Math.Round(shaped * factor * 65535.0);
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort)Math.Min(65535, Math.Max(0, value));
            }

            return ramp;
        }

        /// <summary>
        /// 取主显示设备名。用非托管缓冲区读 DISPLAY_DEVICEW，避开结构体封送的坑
        /// （直接声明结构体的写法在本机拿到过全空字符串）。
        /// </summary>
        private static string QueryPrimaryDeviceName()
        {
            const int Size = 1024;
            IntPtr buf = Marshal.AllocHGlobal(Size);
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    Marshal.WriteInt32(buf, Size);
                    if (!EnumDisplayDevices(null, i, buf, 0)) break;

                    int state = Marshal.ReadInt32(buf, 4 + 64 + 256);   // cb + DeviceName[32] + DeviceString[128]
                    bool attached = (state & 0x1) != 0;                 // DISPLAY_DEVICE_ATTACHED
                    if (!attached && i > 0) continue;

                    string desc = Marshal.PtrToStringUni(buf + 4 + 64, 128) ?? string.Empty;
                    desc = desc.TrimEnd('\0');
                    if (desc.Length > 0) return desc;
                }
                return "未知显示设备";
            }
            catch
            {
                return "未知显示设备";
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }
}
