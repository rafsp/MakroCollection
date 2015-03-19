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
    partial class DesktopService : ServiceBase
    {
        CronScheduler Scheduler = null;

        public DesktopService()
        {
            this.Log = LogManager.GetLogger(typeof(DesktopService));
            Log.Debug("Ctor");
            InitializeComponent();

            this.Task = new DesktopLockTask();
            this.Scheduler = new CronScheduler();
            Scheduler.AddTask(CronParser.ParseExpr("* * * * *"), Task);
        }

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

            Task.Dispose();
        }

        public ILog Log { get; set; }

        public DesktopLockTask Task { get; set; }
    }
}
