using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Makro.Windows.DesktopService.Core;
using Common.Logging;
using CommonUtils.Cron;

namespace Makro.Windows.DesktopService.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof(Program));

            try
            {
                var cs = new CronScheduler();
                using (var dlt = new DesktopLockTask(Environment.UserName))
                {
                    cs.AddTask(CronParser.ParseExpr("*/5 18 * * *"), dlt);
                    cs.Enable();
                    System.Console.Read();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                System.Console.Read();
            }
        }
    }
}
