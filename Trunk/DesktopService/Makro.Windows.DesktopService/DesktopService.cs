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

namespace Makro.Windows.DesktopService
{
    partial class DesktopService : ServiceBase
    {
        CronScheduler Scheduler = null;

        public DesktopService()
        {
            InitializeComponent();

            this.Log = LogManager.GetLogger(typeof(Program));
            var task = new DesktopLockTask();

            this.Scheduler = new CronScheduler();
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), task);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("DesktopService::OnStart");
            Scheduler.Enable();
        }

        protected override void OnPause()
        {
            Log.Debug("DesktopService::OnPause");
            base.OnPause();
            Scheduler.Disable();
        }

        protected override void OnContinue()
        {
            Log.Debug("DesktopService::OnContinue");
            base.OnContinue();
            Scheduler.Enable();
        }

        protected override void OnStop()
        {
            Log.Debug("DesktopService::OnStop");
            if (Scheduler != null)
            {
                Scheduler.Disable();
                Scheduler.Dispose();
            }
        }

        public ILog Log { get; set; }
    }
}
