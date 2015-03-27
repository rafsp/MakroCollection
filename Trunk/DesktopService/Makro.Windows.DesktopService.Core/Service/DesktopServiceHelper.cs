using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Common.Logging;
using System.Configuration;

namespace Makro.Windows.DesktopService.Core.Service
{
    /// <summary>
    /// Implementação do serviço auxiliar do DesktopService. Exposes lock notifications for clients (Agent).
    /// </summary>
    public class DesktopServiceHelper : IDesktopServiceHelper
    {
        /// <summary>
        /// The current user locks
        /// </summary>
        static List<String> Locks = new List<string>();
        /// <summary>
        /// The latest locks by user
        /// </summary>
        static Dictionary<String, DateTime> LatestLocks = new Dictionary<String, DateTime>();
        /// <summary>
        /// The thread synchronization object
        /// </summary>
        static object sync = new object();
        /// <summary>
        /// The delay minutes
        /// </summary>
        static int delayMinutes = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopServiceHelper"/> class.
        /// </summary>
        public DesktopServiceHelper()
        {
            Log = LogManager.GetLogger<DesktopServiceHelper>();
            delayMinutes = int.Parse(ConfigurationManager.AppSettings["DesktopLockDelay"]);
        }

        /// <summary>
        /// Adds an user the lock notification.
        /// </summary>
        /// <param name="user">The user.</param>
        public void AddLock(string user)
        {
            if (!Locks.Contains(user) && (!LatestLocks.ContainsKey(user) || DateTime.Now - LatestLocks[user] > TimeSpan.FromMinutes(delayMinutes)))
            {
                lock (sync)
                {
                    if (!Locks.Contains(user) && (!LatestLocks.ContainsKey(user) || DateTime.Now - LatestLocks[user] > TimeSpan.FromMinutes(delayMinutes)))
                    {
                        Log.DebugFormat("Adding lock notification for user: {0}", user);
                        Locks.Add(user);
                        LatestLocks[user] = DateTime.Now;
                    }
                }
            }
        }
        /// <summary>
        /// Gets and consumes an user lock notification.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public bool GetLock(string user)
        {
            var consumed = false;
            if (Locks.Contains(user))
            {
                lock (sync)
                {
                    if (Locks.Contains(user))
                    {
                        Log.DebugFormat("Consuming lock notification for user: {0}", user);
                        Locks.Remove(user);
                        consumed = true;
                    }
                    
                }
            }

            return consumed;
        }

        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public ILog Log { get; set; }
    }
}
