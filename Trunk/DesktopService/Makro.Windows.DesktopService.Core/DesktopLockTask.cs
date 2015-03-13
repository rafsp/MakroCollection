using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.Cron;
using Common.Logging;
using Makro.Windows.DesktopService.DataAccess;
using Ninject;
using Ninject.Parameters;
using Cassia;

namespace Makro.Windows.DesktopService.Core
{
    public class DesktopLockTask : ITask, IDisposable
    {
        public static StandardKernel DefaultKernel = new StandardKernel(new DI.DIModule());

        public ILog Log { get; set; }
        public LogonHoursDataAccess LogonHoursDataAccess { get; set; }
        public ITerminalServicesManager TerminalServicesManager { get; set; }
        public ITerminalServer Server { get; set; }

        public DesktopLockTask()
        {
            this.Log = LogManager.GetLogger<DesktopLockTask>();
            this.LogonHoursDataAccess = DefaultKernel.Get<LogonHoursDataAccess>();
            this.TerminalServicesManager = new TerminalServicesManager();
            this.Server = this.TerminalServicesManager.GetLocalServer();
        }

        public void Execute()
        {
            try
            {
                Log.DebugFormat("Running task ...");
                //TODO: Check if user is enabled to be blocked (query SGI)

                var allSessions = Server.GetSessions();
                var activeConnections = allSessions.Where(s => s.ConnectionState == ConnectionState.Active);

                foreach (var item in activeConnections)
                {
                    var userMayBeLocked = LogonHoursDataAccess.IsUserLockable(item.UserName); //query
                    if (userMayBeLocked)
                    {
                        Log.InfoFormat("Locking user: {0}", item.UserName);
                        Util.InternalLockWorkstation(true);
                    }
                    else
                    {
                        Log.DebugFormat("User {0} not locked", item.UserName);
                    }
                }

                Log.DebugFormat("Task finished");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void Dispose()
        {
            this.LogonHoursDataAccess.Dispose();
        }
    }
}
