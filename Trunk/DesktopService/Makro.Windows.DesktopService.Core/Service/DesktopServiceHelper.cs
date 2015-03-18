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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DesktopServiceHelper" in both code and config file together.
    public class DesktopServiceHelper : IDesktopServiceHelper
    {
        static List<String> Locks = new List<string>();
        static Dictionary<String, DateTime> LatestLocks = new Dictionary<String, DateTime>();
        static object sync = new object();
        static int delayMinutes = 10;

        public DesktopServiceHelper()
        {
            Log = LogManager.GetLogger<DesktopServiceHelper>();
            delayMinutes = int.Parse(ConfigurationManager.AppSettings["DesktopLockDelay"]);
        }

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

        public ILog Log { get; set; }
    }
}
