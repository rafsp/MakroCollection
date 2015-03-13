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

namespace Makro.Windows.DesktopService
{
    partial class DesktopService : ServiceBase
    {
        CronScheduler Scheduler = null;

        public DesktopService()
        {
            InitializeComponent();
            Scheduler = new CronScheduler();
            var task = new DesktopLockTask("");
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), task);
        }

        protected override void OnStart(string[] args)
        {
            Scheduler.Enable();
        }

        protected override void OnPause()
        {
            base.OnPause();
            Scheduler.Disable();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            Scheduler.Enable();
        }

        protected override void OnStop()
        {
            if (Scheduler != null)
            {
                Scheduler.Disable();
                Scheduler.Dispose();
            }
        }
    }
}
