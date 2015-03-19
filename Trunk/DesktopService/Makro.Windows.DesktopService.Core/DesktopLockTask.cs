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
using System.Threading;
using Makro.Windows.DesktopService.Core.Service;
using System.ServiceModel;
using System.Diagnostics;
using System.Configuration;

namespace Makro.Windows.DesktopService.Core
{
    public class DesktopLockTask : ITask, IDisposable
    {
        public static StandardKernel DefaultKernel = new StandardKernel(new DI.DIModule());

        public ILog Log { get; set; }
        public LogonHoursDataAccess LogonHoursDataAccess { get; set; }
        public ITerminalServicesManager TerminalServicesManager { get; set; }
        public ITerminalServer Server { get; set; }


        public Dictionary<String, Timer> Timers { get; set; }

        public DesktopLockTask()
        {
            this.Log = LogManager.GetLogger<DesktopLockTask>();
            this.LogonHoursDataAccess = DefaultKernel.Get<LogonHoursDataAccess>();
            this.TerminalServicesManager = new TerminalServicesManager();
            this.Server = this.TerminalServicesManager.GetLocalServer();
            this.ChannelFactory = new System.ServiceModel.ChannelFactory<IDesktopServiceHelper>("localEndpoint");
            this.LockDelay = int.Parse(ConfigurationManager.AppSettings["DesktopLockDelay"]);
            this.Timers = new Dictionary<string,Timer>();
        }

        public void Execute()
        {
            try
            {
                Log.DebugFormat("Running task ...");

                var allSessions = Server.GetSessions();
                var activeConnections = allSessions.Where(s => s.ConnectionState == ConnectionState.Active);

                foreach (var item in activeConnections)
                {
                    //EnsureAgent(item);

                    var userMayBeLocked = LogonHoursDataAccess.IsUserLockable(item.UserName, this.LockDelay); //query
                    if (userMayBeLocked && !Timers.ContainsKey(item.UserName))
                    {
                        Log.DebugFormat("Locking user: {0}", item.UserName);
                        AddUserLock(item.UserName);
                        var tc = new TimerCallback(LockUser);
                        var t = new Timer(tc, item, 60 * 1000 * LockDelay, System.Threading.Timeout.Infinite);
                        Timers[item.UserName] = t;
                        //item.Disconnect(true);
                        //Util.InternalLockWorkstation(true);
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

        private void AddUserLock(string user)
        {
            var client = GetHelperClient();
            {
                client.AddLock(user);
                (client as ICommunicationObject).Close();
            }
        }

        private IDesktopServiceHelper GetHelperClient()
        {
            var c = ChannelFactory.CreateChannel();
            return c;
        }

        private void LockUser(object state)
        {
            var s = state as ITerminalServicesSession;
            Log.DebugFormat("Performing lock for user: {0}", s.UserName);
            s.Disconnect(true);

            var t = Timers[s.UserName];
            t.Dispose();
            Timers.Remove(s.UserName);
        }

        public void Dispose()
        {
            this.LogonHoursDataAccess.Dispose();
            this.Server.Dispose();
            this.ChannelFactory.Close();
        }
        public System.ServiceModel.ChannelFactory<IDesktopServiceHelper> ChannelFactory { get; set; }

        public int LockDelay { get; set; }
    }
}
