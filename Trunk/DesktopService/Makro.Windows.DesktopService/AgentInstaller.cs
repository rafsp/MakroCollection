using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using Microsoft.Win32;
using System.ServiceProcess;

namespace Makro.Windows.DesktopService
{
    /// <summary>
    /// Agent installer. Instalador do serviço do Agent.
    /// </summary>
    [RunInstaller(false)]
    public partial class AgentInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentInstaller"/> class.
        /// </summary>
        public AgentInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall" /> event.
        /// Adds the "Allow service to interact with desktop" flag
        /// Start the Agent Service
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have completed their installations.</param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            RegistryKey ckey2 = Registry.LocalMachine.OpenSubKey(
              @"SYSTEM\CurrentControlSet\Services\" + this.serviceInstaller2.ServiceName, true);

            if (ckey2 != null)
            {
                if (ckey2.GetValue("Type") != null)
                {
                    ckey2.SetValue("Type", ((int)ckey2.GetValue("Type") | 256));
                }
            }

            var sc2 = new ServiceController(this.serviceInstaller2.ServiceName);
            sc2.Start();

            base.OnAfterInstall(savedState);
        }
    }
}
