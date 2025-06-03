using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDisplay
{
    internal class ScreenBrightnessControl
    {
        // Windows API 声明
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP ramp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue;
        }

        /// <summary>
        /// 设置屏幕亮度（0-100）
        /// </summary>
        /// <param name="brightness">亮度值，0-100</param>
        public static void SetBrightness(int brightness)
        {
            if (brightness < 0 || brightness > 100)
                throw new ArgumentException("Brightness must be between 0 and 100");

            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                throw new Exception("Unable to get device context");

            RAMP ramp = new RAMP
            {
                Red = new ushort[256],
                Green = new ushort[256],
                Blue = new ushort[256]
            };

            // 计算伽马曲线
            for (int i = 0; i < 256; i++)
            {
                int value = i * (brightness * 256) / 100;
                value = Math.Min(65535, Math.Max(0, value));
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort)value;
            }

            SetDeviceGammaRamp(hdc, ref ramp);
        }







        // 设置亮度（API，优化伽马曲线）
        public static void SetBrightnessAPI(int brightness)
        {
            if (brightness < 0 || brightness > 100)
                throw new ArgumentException("Brightness must be between 0 and 100");

            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                throw new Exception("Unable to get device context");

            RAMP ramp = new RAMP
            {
                Red = new ushort[256],
                Green = new ushort[256],
                Blue = new ushort[256]
            };

            // 使用非线性伽马曲线（指数调整）
            double gamma = brightness / 100.0; // 0.0 到 1.0
            double exponent = 3; // 调整指数，放大低亮度变化 2.5
            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double adjusted = Math.Pow(normalized, 1 / exponent) * gamma;
                int value = (int)(adjusted * 65535);
                value = Math.Min(65535, Math.Max(0, value));
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort)value;
            }

            SetDeviceGammaRamp(hdc, ref ramp);
        }
    }
}
