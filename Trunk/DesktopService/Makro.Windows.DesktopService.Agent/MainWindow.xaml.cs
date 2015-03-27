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
    /// Interaction logic for MainWindow.xaml.
    /// Logica de interação para a MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.TSM = new TerminalServicesManager();
            this.BackGroundWorker = new BackgroundWorker();
            this.BackGroundWorker.DoWork += new DoWorkEventHandler(BackGroundWorker_DoWork);
            this.BackGroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackGroundWorker_RunWorkerCompleted);

            var timerMins = ConfigurationManager.AppSettings["AddLockDurationMins"];
            var timerMS = int.Parse(timerMins) * 1000;

            var warningLabel = ConfigurationManager.AppSettings["WarningText"];
            this.txtBlockWarning.Text = String.Format(warningLabel, timerMins);

            this.Timer = new System.Timers.Timer(timerMS);
            this.Timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            this.Timer.Enabled = true;

            this.Sync = SynchronizationContext.Current;

            this.Hide();
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the BackGroundWorker control.
        /// Shows the warning when a pending notification is found.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        void BackGroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                this.Sync.Post(state => this.Show(), null);
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.BackGroundWorker.IsBusy)
                this.BackGroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the DoWork event of the BackGroundWorker control.
        /// Checks for pending notifications for each current active user.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Gets or sets the back ground worker.
        /// </summary>
        /// <value>
        /// The back ground worker.
        /// </value>
        public BackgroundWorker BackGroundWorker { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the timer.
        /// </summary>
        /// <value>
        /// The timer.
        /// </value>
        public System.Timers.Timer Timer { get; set; }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// Hides the warning
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Gets or sets the TSM.
        /// </summary>
        /// <value>
        /// The TSM.
        /// </value>
        public TerminalServicesManager TSM { get; set; }

        /// <summary>
        /// Gets or sets the synchronize.
        /// </summary>
        /// <value>
        /// The synchronize.
        /// </value>
        public SynchronizationContext Sync { get; set; }
    }
}
