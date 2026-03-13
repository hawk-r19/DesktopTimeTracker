using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimeTracker
{
    class VirtualDesktop
    {

        [DllImport("VirtualDesktopAccessor.dll")]
        public static extern int RegisterPostMessageHook(IntPtr listenerHwnd, uint messageOffset);

        [DllImport("VirtualDesktopAccessor.dll")]
        public static extern int GetCurrentDesktopNumber();

        [DllImport("VirtualDesktopAccessor.dll")]
        public static extern int GetDesktopCount();
    }
}
