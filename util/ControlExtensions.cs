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
            if (ctrl == null || ctrl.IsDisposed) return;
            if (ctrl.InvokeRequired)
                ctrl.Invoke(action);
            else
                action();
        }
    }
}
