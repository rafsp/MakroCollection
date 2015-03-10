using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Makro.Windows.DesktopService.Core
{
    public class Util
    {
        [DllImport("user32")]
        protected static extern void LockWorkStation();

        public static void InternalLockWorkstation(bool _lock)
        {
            if (_lock)
            {
                LockWorkStation();
            }
        }
    }
}
