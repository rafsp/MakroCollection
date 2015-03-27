using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using System.Data.Common;
using System.Configuration;
using Makro.Windows.DesktopService.DataAccess;

namespace Makro.Windows.DesktopService.Core.DI
{
    /// <summary>
    /// Dependency injection module.
    /// Módulo de DI padrão.
    /// </summary>
    public class DIModule : NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            ////Oracle.DataAccess.Client.OracleClientFactory
            
            //var defaultDbProvider = ConfigurationManager.AppSettings["defaultDbProvider"];
            //var defaultDbProviderType = Type.GetType(defaultDbProvider);

            //this.Bind<DbProviderFactory>().To(defaultDbProviderType);
            this.Bind<LogonHoursDataAccess>().To<LogonHoursDataAccess>();
        }
    }
}
