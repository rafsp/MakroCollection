using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.Cron;
using Common.Logging;
using System.Diagnostics;

namespace Makro.Windows.DesktopService.Core
{
    public class DesktopAgentTask : ITask, IDisposable
    {
        public ILog Log { get; set; }
        public List<Process> ProcessList { get; set; }

        public DesktopAgentTask(string userName)
        {
            this.Log = LogManager.GetLogger<DesktopAgentTask>();
            this.UserName = userName;
            this.ProcessList = new List<Process>();
        }

        public void Execute()
        {
            try
            {
                const string agentImageName = "Makro.Windows.DesktopService.Agent";
                var ps = Process.GetProcesses();
                var runningAgent = ps.FirstOrDefault(p => p.ProcessName == agentImageName);

                if (runningAgent == null)
                {
                    Log.Debug("DesktopService::OnStart::InitializingAgent");
                    var psi = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "\\" + agentImageName + ".exe");
                    psi.UseShellExecute = false;;
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

        public void Dispose()
        {
            foreach (var p in ProcessList)
            {
                p.Kill();
            }
        }

        public string UserName { get; set; }
    }
}
