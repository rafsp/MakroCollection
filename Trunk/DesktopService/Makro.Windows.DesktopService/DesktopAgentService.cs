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
    partial class DesktopAgentService : ServiceBase
    {
        CronScheduler Scheduler = null;

        public DesktopAgentService()
        {
            this.Log = LogManager.GetLogger(typeof(DesktopAgentService));
            Log.Debug("Ctor");
            InitializeComponent();

            this.Task = new DesktopAgentTask(Environment.UserName);
            this.Scheduler = new CronScheduler();
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), Task);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("DesktopAgent::OnStart");
            Log.DebugFormat("Args: ", String.Join(" ", args));
            this.Scheduler.Enable();
        }

        protected override void OnStop()
        {
            Log.Debug("DesktopAgent::OnStop");
            this.Scheduler.Disable();
            this.Scheduler.Dispose();
            this.Task.Dispose();
        }

        public DesktopAgentTask Task { get; set; }

        public ILog Log { get; set; }
    }
}
