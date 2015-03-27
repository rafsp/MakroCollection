using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;


namespace Makro.Windows.DesktopService
{
    /// <summary>
    /// DesktopService Windows Service installer.
    /// Instalador do serviço windows DesktopService.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall" /> event.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have completed their installations.</param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            RegistryKey ckey = Registry.LocalMachine.OpenSubKey(
              @"SYSTEM\CurrentControlSet\Services\" + this.serviceInstaller1.ServiceName, true);
            
            if (ckey != null)
            {
                if (ckey.GetValue("Type") != null)
                {
                    ckey.SetValue("Type", ((int)ckey.GetValue("Type") | 256));
                }
            }

            var sc = new ServiceController(this.serviceInstaller1.ServiceName);
            sc.Start();

            base.OnAfterInstall(savedState);
        }
    }
}
