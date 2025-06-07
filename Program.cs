using System.Runtime.InteropServices;

namespace WinDisplay
{
    internal static class Program
    {

        // ���� Win32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        // ���� FindWindow ����
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "Global\\BethVgs7KY8McUFx0dDdUsD6Fl2v1lc1GG2fhvtgDT4HZAmuoZ2vKrvzb4kqpgfj";
            bool createdNew;

            using (Mutex mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (createdNew)
                {
                 
                    ApplicationConfiguration.Initialize();
                    Application.Run(new Form1());
                }
                else
                {
                    // ����ʵ�������Լ����
                    IntPtr hWnd = FindWindow(null, "YourFormTitle"); // �滻Ϊ��Ĵ������
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE); // ��ԭ���ڣ������С����
                        SetForegroundWindow(hWnd); // �����
                    }
                    else
                    {
                        MessageBox.Show("�����������У��޷������ڶ���ʵ����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }
            }
        }
    }
}