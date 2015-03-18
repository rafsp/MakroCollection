using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Makro.Windows.DesktopService.Core.Service
{
    [ServiceContract]
    public interface IDesktopServiceHelper
    {
        [OperationContract]
        void AddLock(string user);

        [OperationContract]
        bool GetLock(string user);
    }
}
