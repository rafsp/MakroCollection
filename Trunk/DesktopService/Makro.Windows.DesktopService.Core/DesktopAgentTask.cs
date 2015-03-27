using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.Cron;
using Common.Logging;
using System.Diagnostics;

namespace Makro.Windows.DesktopService.Core
{
    /// <summary>
    /// Task that ensures an running Agent for each active user.
    /// Tarefa que garante a execução de um Agent para cada usuário ativo.
    /// </summary>
    [Obsolete]
    public class DesktopAgentTask : ITask, IDisposable
    {
        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public ILog Log { get; set; }
        /// <summary>
        /// Gets or sets the process list.
        /// </summary>
        /// <value>
        /// The process list.
        /// Stores the 
        /// </value>
        public List<Process> ProcessList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAgentTask"/> class.
        /// </summary>
        /// <param name="userName">Name of the current active user.</param>
        public DesktopAgentTask(string userName)
        {
            this.Log = LogManager.GetLogger<DesktopAgentTask>();
            this.UserName = userName;
            this.ProcessList = new List<Process>();
        }

        /// <summary>
        /// Executes this instance.
        /// Verifies and starts an Agent if necessary
        /// </summary>
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

        /// <summary>
        /// Kill the started processes
        /// </summary>
        public void Dispose()
        {
            foreach (var p in ProcessList)
            {
                p.Kill();
            }
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
    }
}
