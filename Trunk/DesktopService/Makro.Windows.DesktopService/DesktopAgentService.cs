using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Makro.Windows.DesktopService.Core;
using CommonUtils.Cron;
using Common.Logging;

namespace Makro.Windows.DesktopService
{
    /// <summary>
    /// Windows Service that ensure a running agent process for each active users.
    /// Serviço windows que garante a execução de um Agent para cada usuário ativo.
    /// </summary>
    partial class DesktopAgentService : ServiceBase
    {
        /// <summary>
        /// The scheduler
        /// </summary>
        CronScheduler Scheduler = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAgentService"/> class.
        /// 
        /// </summary>
        public DesktopAgentService()
        {
            this.Log = LogManager.GetLogger(typeof(DesktopAgentService));
            Log.Debug("Ctor");
            InitializeComponent();

            this.Task = new DesktopAgentTask(Environment.UserName);
            this.Scheduler = new CronScheduler();
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), Task);
        }

        /// <summary>
        /// Starts the service
        /// Starts the scheduler
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            Log.Debug("DesktopAgent::OnStart");
            Log.DebugFormat("Args: ", String.Join(" ", args));
            this.Scheduler.Enable();
        }

        /// <summary>
        /// Stops the service
        /// Stops the scheduler
        /// </summary>
        protected override void OnStop()
        {
            Log.Debug("DesktopAgent::OnStop");
            this.Scheduler.Disable();
            this.Scheduler.Dispose();
            this.Task.Dispose();
        }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public DesktopAgentTask Task { get; set; }

        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public ILog Log { get; set; }
    }
}
