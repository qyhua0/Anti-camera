using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDisplay.util
{
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control ctrl, Action action)
        {
            if (ctrl == null) return;
            if (ctrl.IsDisposed || !ctrl.IsHandleCreated) return;

            try
            {
                if (ctrl.InvokeRequired)
                {
                    if (!ctrl.IsDisposed && ctrl.IsHandleCreated)
                        ctrl.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
                // 窗口已经关闭，忽略即可
            }
            catch (InvalidOperationException)
            {
                // 控件句柄无效，也说明窗口关闭了，忽略
            }
        }

    }
}
