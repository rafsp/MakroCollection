using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Makro.Windows.DesktopService.Core.Tests
{
    [TestClass]
    public class UtilTests
    {
        [TestMethod]
        public void Util_LockWorkstation()
        {
            Util.InternalLockWorkstation(false);
        }
    }
}
