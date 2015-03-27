using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Makro.Windows.DesktopService.Core
{
    /// <summary>
    /// Provides utility functions.
    /// Fornece funções utilitárias.
    /// </summary>
    [Obsolete]
    public class Util
    {
        /// <summary>
        /// Locks the work station.
        /// Bloqueia a sessão atual do usuário usando a API do Windows.
        /// </summary>
        [DllImport("user32")]
        protected static extern void LockWorkStation();

        /// <summary>
        /// Internals the lock workstation.
        /// </summary>
        /// <param name="_lock">if set to <c>true</c>, locks the user.</param>
        public static void InternalLockWorkstation(bool _lock)
        {
            if (_lock)
            {
                LockWorkStation();
            }
        }
    }
}
