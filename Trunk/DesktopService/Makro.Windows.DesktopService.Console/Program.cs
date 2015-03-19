using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Makro.Windows.DesktopService.Core;
using Common.Logging;
using CommonUtils.Cron;
using System.ServiceModel;

namespace Makro.Windows.DesktopService.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof(Program));

            System.Console.Write("Starting the helper service ...");
            var sh = new ServiceHost(typeof(Makro.Windows.DesktopService.Core.Service.DesktopServiceHelper));
            sh.Open();
            System.Console.WriteLine("OK");
            
            try
            {
                using (var dlt = new DesktopLockTask())
                    dlt.Execute();

              //  var cs = new CronScheduler();
              //  using (var dlt = new DesktopLockTask())
              //  {
              //      //cs.AddTask(CronParser.ParseExpr("*/5 18 * * *"), dlt);
              //      cs.AddTask(CronParser.ParseExpr("* * * * *"), dlt);
              //      cs.Enable();
              //      System.Console.Read();
              //  }

                System.Console.Read();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                System.Console.Read();
            }
        }
    }
}
