using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Positron
{
    internal static class DispatcherExtensions
    {

        public static void DispatchTask(this IDispatcher dispatcher, Func<Task> action)
        {
            dispatcher.Dispatch(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }
        public static void DispatchTaskDelayed(this IDispatcher dispatcher, TimeSpan delay, Func<Task> action)
        {
            dispatcher.DispatchDelayed(delay, async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }



    }
}
