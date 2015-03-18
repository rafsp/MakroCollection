using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Timers;
using Cassia;
using System.ServiceModel;
using Makro.Windows.DesktopService.Core.Service;
using System.Threading;
using System.Configuration;

namespace Makro.Windows.DesktopService.Agent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            this.TSM = new TerminalServicesManager();
            this.BackGroundWorker = new BackgroundWorker();
            this.BackGroundWorker.DoWork += new DoWorkEventHandler(BackGroundWorker_DoWork);
            this.BackGroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackGroundWorker_RunWorkerCompleted);

            var timerMins = ConfigurationManager.AppSettings["AddLockDurationMins"];
            var timerMS = int.Parse(timerMins) * 1000;

            this.Timer = new System.Timers.Timer(timerMS);
            this.Timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            this.Timer.Enabled = true;

            this.Sync = SynchronizationContext.Current;

            this.Hide();
        }

        void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                this.Sync.Post(state => this.Show(), null);
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.BackGroundWorker.IsBusy)
                this.BackGroundWorker.RunWorkerAsync();
        }

        void BackGroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                e.Result = false;

                try
                {
                    var s = TSM.GetLocalServer();
                    var a = s.GetSessions().FirstOrDefault(ss => ss.ConnectionState == ConnectionState.Active);

                    var cf = new ChannelFactory<IDesktopServiceHelper>("localEndpoint");
                    var c = cf.CreateChannel();
                    e.Result = c.GetLock(a.UserName);
                }
                catch (Exception ex)
                {   
                    Console.WriteLine(ex.Message);
                }

                IsRunning = false;
            }
        }

        public BackgroundWorker BackGroundWorker { get; set; }
        public bool IsRunning { get; set; }

        public System.Timers.Timer Timer { get; set; }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public TerminalServicesManager TSM { get; set; }

        public SynchronizationContext Sync { get; set; }
    }
}
