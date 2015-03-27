using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Makro.Windows.DesktopService.Core.Service
{
    /// <summary>
    /// DesktopService helper service contract.
    /// Contrato de serviço auxiliar do DesktopService.
    /// </summary>
    [ServiceContract]
    public interface IDesktopServiceHelper
    {
        /// <summary>
        /// Adds the lock.
        /// </summary>
        /// <param name="user">The user.</param>
        [OperationContract]
        void AddLock(string user);

        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        [OperationContract]
        bool GetLock(string user);
    }
}
