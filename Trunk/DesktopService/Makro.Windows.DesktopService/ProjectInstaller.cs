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
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

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
