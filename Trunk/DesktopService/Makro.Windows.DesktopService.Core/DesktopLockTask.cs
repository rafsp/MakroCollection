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

namespace Makro.Windows.DesktopService.Core
{
    public class DesktopLockTask : ITask, IDisposable
    {
        public static StandardKernel DefaultKernel = new StandardKernel(new DI.DIModule());

        public ILog Log { get; set; }
        public LogonHoursDataAccess LogonHoursDataAccess { get; set; }
        public ITerminalServicesManager TerminalServicesManager { get; set; }
        public ITerminalServer Server { get; set; }
        public List<Process> ProcessList { get; set; }

        public DesktopLockTask()
        {
            this.Log = LogManager.GetLogger<DesktopLockTask>();
            this.LogonHoursDataAccess = DefaultKernel.Get<LogonHoursDataAccess>();
            this.TerminalServicesManager = new TerminalServicesManager();
            this.Server = this.TerminalServicesManager.GetLocalServer();
            this.ChannelFactory = new System.ServiceModel.ChannelFactory<IDesktopServiceHelper>("localEndpoint");
            this.ProcessList = new List<Process>();
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
                    var userMayBeLocked = LogonHoursDataAccess.IsUserLockable(item.UserName); //query
                    if (userMayBeLocked)
                    {
                        Log.DebugFormat("Locking user: {0}", item.UserName);
                        AddUserLock(item.UserName);
                        var tc = new TimerCallback(LockUser);
                        new Timer(tc, item, 60 * 1000 * 10, System.Threading.Timeout.Infinite);
                        //item.Disconnect(true);
                        //Util.InternalLockWorkstation(true);

                        EnsureAgent(item);
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

        private void EnsureAgent(ITerminalServicesSession item)
        {
            try
            {
                const string agentImageName = "Makro.Windows.DesktopService.Agent.exe";
                var ps = item.GetProcesses();
                var runningAgent = ps.FirstOrDefault(p => p.ProcessName == agentImageName);

                if (runningAgent == null)
                {
                    Log.Debug("DesktopService::OnStart::InitializingAgent");
                    var psi = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "\\" + agentImageName);
                    psi.UserName = item.UserName;
                    this.ProcessList.Add(Process.Start(psi));
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error starting the agent. Base: " + AppDomain.CurrentDomain.BaseDirectory);
                Log.Error(ex);
                throw;
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
        }

        public void Dispose()
        {
            this.LogonHoursDataAccess.Dispose();
            this.Server.Dispose();
            this.ChannelFactory.Close();

            foreach (var p in ProcessList)
            {
                p.Kill();
            }
        }
        public System.ServiceModel.ChannelFactory<IDesktopServiceHelper> ChannelFactory { get; set; }
    }
}
