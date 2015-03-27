using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using CommonUtils.Cron;
using Makro.Windows.DesktopService.Core;
using Common.Logging;
using System.ServiceModel;
using Makro.Windows.DesktopService.Core.Service;

namespace Makro.Windows.DesktopService
{
    /// <summary>
    /// Windows Service that monitors and disconnects the current active user when AD LogonHours field for that user indicates the disabled state.
    /// Serviço windows que monitora e desconecta o usuário ativo quando o campo LogonHours do AD para esse usuário apresenta o estado desabilitado.
    /// </summary>
    partial class DesktopService : ServiceBase
    {
        /// <summary>
        /// The scheduler
        /// </summary>
        CronScheduler Scheduler = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopService"/> class.
        /// </summary>
        public DesktopService()
        {
            this.Log = LogManager.GetLogger(typeof(DesktopService));
            Log.Debug("Ctor");
            InitializeComponent();

            this.Task = new DesktopLockTask();
            this.Scheduler = new CronScheduler();
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), Task);
        }

        /// <summary>
        /// Starts the service
        /// Enables the verification scheduler(timer)
        /// Opens the helper service to send notifications to.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            Log.Debug("DesktopService::OnStart");
            Log.DebugFormat("Args: ", String.Join(" ", args));
            Scheduler.Enable();

            try
            {
                var sh = new ServiceHost(typeof(DesktopServiceHelper));
                sh.Open();
            }
            catch (Exception ex)
            {
                Log.Error("Error opening the server");
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Disables the scheduler
        /// </summary>
        protected override void OnPause()
        {
            Log.Debug("DesktopService::OnPause");
            base.OnPause();
            Scheduler.Disable();
        }

        /// <summary>
        /// Enables the scheduler
        /// </summary>
        protected override void OnContinue()
        {
            Log.Debug("DesktopService::OnContinue");
            base.OnContinue();
            Scheduler.Enable();
        }

        /// <summary>
        /// Stops the service
        /// Disables the scheduler
        /// </summary>
        protected override void OnStop()
        {
            Log.Debug("DesktopService::OnStop");
            if (Scheduler != null)
            {
                Scheduler.Disable();
                Scheduler.Dispose();
            }

            Task.Dispose();
        }

        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public ILog Log { get; set; }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public DesktopLockTask Task { get; set; }
    }
}
