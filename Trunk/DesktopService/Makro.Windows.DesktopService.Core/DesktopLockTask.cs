using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils.Cron;
using Common.Logging;
using Makro.Windows.DesktopService.DataAccess;
using Ninject;
using Ninject.Parameters;
using Cassia;
using System.Threading;
using Makro.Windows.DesktopService.Core.Service;
using System.ServiceModel;
using System.Diagnostics;
using System.Configuration;

namespace Makro.Windows.DesktopService.Core
{
    /// <summary>
    /// Task for checking LogonHours info of each active user and locking(disconnecting) their session when necessary
    /// Tarefa que checa o campo LogonHours de cara usuário ativo e bloqueia sua sessão quando necessário.
    /// </summary>
    public class DesktopLockTask : ITask, IDisposable
    {
        /// <summary>
        /// Ninject DI kernel
        /// Kernel de DI do Ninject
        /// </summary>
        public static StandardKernel DefaultKernel = new StandardKernel(new DI.DIModule());

        /// <summary>
        /// Gets or sets the log.
        /// Obtém ou atribui o objeto de log.
        /// </summary>
        /// <value>
        /// The log.
        /// Objeto de log.
        /// </value>
        public ILog Log { get; set; }
        /// <summary>
        /// Gets or sets the logon hours data access object.
        /// Obtém ou atribui o objeto de acesso a dados do logon hours.
        /// </summary>
        /// <value>
        /// The logon hours data access object.
        /// O objeto de acesso a dados do logon hours
        /// </value>
        public LogonHoursDataAccess LogonHoursDataAccess { get; set; }
        /// <summary>
        /// Gets or sets the terminal services manager.
        /// Obtém ou atribui o gerenciador do Terminal Services.
        /// </summary>
        /// <value>
        /// The terminal services manager.
        /// Gerenciador do Terminal Services
        /// </value>
        public ITerminalServicesManager TerminalServicesManager { get; set; }
        /// <summary>
        /// Gets or sets the terminal server info object.
        /// Obtém ou atribui o objeto Terminal Server.
        /// </summary>
        /// <value>
        /// The terminal server info object.
        /// Objeto Terminal Server.
        /// </value>
        public ITerminalServer Server { get; set; }


        /// <summary>
        /// Gets or sets the timers.
        /// Obtém ou atribui os timers
        /// </summary>
        /// <value>
        /// The user locking timers (when the lock is delayed).
        /// Timers de bloqueio de usuários (Quando o bloqueio é atrasado)
        /// </value>
        public Dictionary<String, Timer> Timers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopLockTask"/> class.
        /// Construtor
        /// </summary>
        public DesktopLockTask()
        {
            this.Log = LogManager.GetLogger<DesktopLockTask>();
            this.LogonHoursDataAccess = DefaultKernel.Get<LogonHoursDataAccess>();
            this.TerminalServicesManager = new TerminalServicesManager();
            this.Server = this.TerminalServicesManager.GetLocalServer();
            this.ChannelFactory = new System.ServiceModel.ChannelFactory<IDesktopServiceHelper>("localEndpoint");
            this.LockDelay = int.Parse(ConfigurationManager.AppSettings["DesktopLockDelay"]);
            this.Timers = new Dictionary<string,Timer>();
        }

        /// <summary>
        /// Executes this instance.
        /// Executa a tarefa.
        /// </summary>
        public void Execute()
        {
            try
            {
                Log.DebugFormat("Running task ...");

                var allSessions = Server.GetSessions();
                var activeConnections = allSessions.Where(s => s.ConnectionState == ConnectionState.Active);

                foreach (var item in activeConnections)
                {
                    //EnsureAgent(item);

                    var userMayBeLocked = LogonHoursDataAccess.IsUserLockable(item.UserName, this.LockDelay); //query
                    if (userMayBeLocked && !Timers.ContainsKey(item.UserName))
                    {
                        Log.DebugFormat("Locking user: {0}", item.UserName);
                        AddUserLock(item.UserName);
                        var tc = new TimerCallback(LockUser);
                        var t = new Timer(tc, item, 60 * 1000 * LockDelay, System.Threading.Timeout.Infinite);
                        Timers[item.UserName] = t;
                        //item.Disconnect(true);
                        //Util.InternalLockWorkstation(true);
                    }
                    else
                    {
                        Log.DebugFormat("User {0} not locked", item.UserName);
                    }
                }

                Log.DebugFormat("Task finished");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Adds the user lock.
        /// Cria uma notificação de bloqueio de usuário.
        /// Essa notificação poderá ser lida/consumida pelo Agent.
        /// </summary>
        /// <param name="user">The user.</param>
        private void AddUserLock(string user)
        {
            var client = GetHelperClient();
            {
                client.AddLock(user);
                (client as ICommunicationObject).Close();
            }
        }

        /// <summary>
        /// Gets the helper client.
        /// Obtem o client do serviço de notificação
        /// </summary>
        /// <returns></returns>
        private IDesktopServiceHelper GetHelperClient()
        {
            var c = ChannelFactory.CreateChannel();
            return c;
        }

        /// <summary>
        /// Locks the user.
        /// Blqueia a sessão do usuário.
        /// </summary>
        /// <param name="state">The state.</param>
        private void LockUser(object state)
        {
            var s = state as ITerminalServicesSession;
            Log.DebugFormat("Performing lock for user: {0}", s.UserName);
            s.Disconnect(true);

            var t = Timers[s.UserName];
            t.Dispose();
            Timers.Remove(s.UserName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Libera os objetos instanciados.
        /// </summary>
        public void Dispose()
        {
            this.LogonHoursDataAccess.Dispose();
            this.Server.Dispose();
            this.ChannelFactory.Close();
        }
        /// <summary>
        /// Gets or sets the channel factory.
        /// Obtém ou atribui o ChannelFactory do serviço de notificação
        /// </summary>
        /// <value>
        /// The channel factory.
        /// </value>
        public System.ServiceModel.ChannelFactory<IDesktopServiceHelper> ChannelFactory { get; set; }

        /// <summary>
        /// Gets or sets the lock delay.
        /// Obtem ou atribui o atraso (em minutos) de bloqueio. [AppSettings: DesktopLockDelay]
        /// </summary>
        /// <value>
        /// The lock delay.
        /// </value>
        public int LockDelay { get; set; }
    }
}
