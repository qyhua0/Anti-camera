/*
 * x-保护 (WinDisplay) v2.0 —— 屏幕隐私防护工具
 * 文件：util/ControlExtensions.cs
 * 作者：zhenhua
 * 说明：Control 的扩展方法：InvokeIfRequired——后台线程安全地回 UI 线程更新控件，
 * 窗口已关闭（句柄销毁 / ObjectDisposedException）时静默丢弃，不让关窗竞态把程序炸掉。
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDisplay.util
{
    /// <summary>
    /// <see cref="Control"/> 的小扩展。单独开一个文件，是为了让所有后台线程（BackgroundWorker、
    /// Task、抓包线程）回 UI 线程时走同一套关窗容错逻辑，而不是每个窗体自己拼一段
    /// <c>if (InvokeRequired)</c>。
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// 在控件自己的线程上执行 <paramref name="action"/>：已经在 UI 线程就直接跑，
        /// 否则用 <c>Invoke</c> 同步投递过去（保证进度文本不会跨线程乱序）。
        /// <para>
        /// 两种情况下会静默丢弃：控件已释放，或句柄还没建（窗体还没 Show 完就有后台回调）。
        /// <see cref="ObjectDisposedException"/> 与 <see cref="InvalidOperationException"/> 也吞掉：
        /// 这两个只意味着「窗口正好在这一瞬间关了」，后台进度不应该能把正在关的程序抛崩。
        /// </para>
        /// </summary>
        /// <param name="ctrl">目标控件，用它拿 UI 线程上下文。</param>
        /// <param name="action">要在 UI 线程上跑的动作。这里不抦它里的业务异常，那种错该照旧往上冒。</param>
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
