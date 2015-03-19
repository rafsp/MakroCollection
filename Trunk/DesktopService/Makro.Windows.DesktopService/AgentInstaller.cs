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
    [RunInstaller(false)]
    public partial class AgentInstaller : System.Configuration.Install.Installer
    {
        public AgentInstaller()
        {
            InitializeComponent();
        }

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
