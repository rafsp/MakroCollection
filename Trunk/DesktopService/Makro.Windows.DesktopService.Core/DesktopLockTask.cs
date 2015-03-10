using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.Cron;
using Common.Logging;
using Makro.Windows.DesktopService.DataAccess;
using Ninject;
using Ninject.Parameters;

namespace Makro.Windows.DesktopService.Core
{
    public class DesktopLockTask : ITask, IDisposable
    {
        public static StandardKernel DefaultKernel = new StandardKernel(new DI.DIModule());

        public string Username { get; set; }
        public string ConnectionString { get; set; }
        public ILog Log { get; set; }
        public SGIDataAccess SGIDataAccess { get; set; }

        public DesktopLockTask(string user, string connectionString = "DesktopLockTaskConnectionString")
        {
            this.Log = LogManager.GetLogger(typeof(DesktopLockTask));
            this.Username = user;
            this.ConnectionString = connectionString;

            this.SGIDataAccess = DefaultKernel.Get<SGIDataAccess>(
                new ConstructorArgument("connectionString", connectionString, true)
            );

            Log.DebugFormat("User: {0}", this.Username);
        }

        public void Execute()
        {
            try
            {
                Log.DebugFormat("Running task ...");
                //TODO: Check if user is enabled to be blocked (query SGI)
                var userMayBeLocked = SGIDataAccess.IsUserLockable(this.Username); //query

                if (userMayBeLocked)
                {
                    Log.DebugFormat("Locking user");
                    Util.InternalLockWorkstation(true);
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
            this.SGIDataAccess.Dispose();
        }
    }
}
