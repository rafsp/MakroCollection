using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Common.Logging;

namespace Makro.Windows.DesktopService
{
    /// <summary>
    /// Service executable.
    /// Executável do serviço.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var Log = LogManager.GetLogger(typeof(Program));
            Log.Debug("Creating service context");

            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new DesktopService()
			    };
                ServiceBase.Run(ServicesToRun);
            }
            catch(Exception ex) 
            {
                Log.Error(ex);
            }
        }
    }
}
