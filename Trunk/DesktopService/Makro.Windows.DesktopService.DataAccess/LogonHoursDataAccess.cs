using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Configuration;
using System.Data.Common;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using Common.Logging;

namespace Makro.Windows.DesktopService.DataAccess
{
    public class LogonHoursDataAccess : IDisposable
    {
        public DbConnection Connection { get; set; }
        public DirectorySearcher DirectorySearcher { get; set; }
        public Dictionary<string, byte[]> LogonHoursByUserCache { get; set; }
        public ILog Log { get; set; }

        public LogonHoursDataAccess()
        {
            this.Log = LogManager.GetLogger<LogonHoursDataAccess>();
            this.DirectorySearcher = new DirectorySearcher();
            this.LogonHoursByUserCache = new Dictionary<string, byte[]>();
        }

        public bool IsUserLockable(string user)
        {
            byte[] bytes = null;
            try {
                bytes = GetUserLogonHours(user);
            }
            catch (Exception ex) {
                Log.Warn("Error when trying to get user logon hours from AD.", ex);
            }

            if (bytes == null && this.LogonHoursByUserCache.ContainsKey(user)) 
            {
                Log.InfoFormat("Trying to get LogonHours from the cache. {0}", user);
                bytes = this.LogonHoursByUserCache[user];
            }

            if (bytes == null)
            {
                Log.WarnFormat("Can't get LogonHours for the user {0}. User will not be blocked right now.", user);
                return false;
            }

            DateTime? dtStart;
            DateTime? dtEnd;

            GetTodayWorkPeriod(bytes, out dtStart, out dtEnd);

            if (DateTime.Now < dtStart || DateTime.Now > dtEnd)
                return true;
            

            return false;
        }

        private byte[] GetUserLogonHours(string user)
        {
            var path = "LDAP://DC=MAKRO,DC=COM,DC=BR";
            var root = new DirectoryEntry(path);
            var search = new DirectorySearcher(root);
            search.Filter = String.Format("(&(objectClass=user)(samAccountName={0}))", user);
            search.PropertiesToLoad.Add("logonHours");

            var results = search.FindAll();
            byte[] bytes = null;

            foreach (SearchResult r in results)
            {
                if (r.Properties.Contains("logonHours"))
                {
                    Log.DebugFormat("User {0} does have LogonHours info.", user);
                    bytes = r.Properties["logonHours"][0] as byte[];

                    //update the local cache
                    this.LogonHoursByUserCache[user] = bytes;
                }
                else
                {
                    Log.DebugFormat("User {0} doesn't have LogonHours info.", user);
                }
            }
            return bytes;
        }

        private void GetTodayWorkPeriod(byte[] bytes, out DateTime? dtStart, out DateTime? dtEnd)
        {
            var r = DecodeLogonHours(bytes);

            var now = DateTime.Now;
            var todayLogonHours = r.FirstOrDefault(lh => lh.Key == now.DayOfWeek);

            if (todayLogonHours.Key == now.DayOfWeek)
            {
                dtStart = new DateTime(now.Year, now.Month, now.Day, Convert.ToInt32(todayLogonHours.Value.First()), 0, 0);
                dtEnd = new DateTime(now.Year, now.Month, now.Day, Convert.ToInt32(todayLogonHours.Value.Last()), 59, 59);
            }
            else
            {
                dtStart = null;
                dtEnd = null;
            }
        }

        private Dictionary<DayOfWeek, List<String>> DecodeLogonHours(byte[] bytes)
        {
            var rtrn = new Dictionary<DayOfWeek, List<String>>();
            int min;
            int max;

            //Sunday
            var _0to7 = bytes[1];
            var _8to15 = bytes[2];
            var _16to23 = bytes[3];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Sunday, new List<string>() { min.ToString(), max.ToString() });

            //Monday
            _0to7 = bytes[4];
            _8to15 = bytes[5];
            _16to23 = bytes[6];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Monday, new List<string>() { min.ToString(), max.ToString() });

            //Tuesday
            _0to7 = bytes[7];
            _8to15 = bytes[8];
            _16to23 = bytes[9];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Tuesday, new List<string>() { min.ToString(), max.ToString() });

            //Wednesday
            _0to7 = bytes[10];
            _8to15 = bytes[11];
            _16to23 = bytes[12];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Wednesday, new List<string>() { min.ToString(), max.ToString() });

            //Thursday
            _0to7 = bytes[13];
            _8to15 = bytes[14];
            _16to23 = bytes[15];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Thursday, new List<string>() { min.ToString(), max.ToString() });

            //Friday
            _0to7 = bytes[16];
            _8to15 = bytes[17];
            _16to23 = bytes[18];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Friday, new List<string>() { min.ToString(), max.ToString() });

            //Saturday
            _0to7 = bytes[19];
            _8to15 = bytes[20];
            _16to23 = bytes[0];
            DecodeDay(_0to7, _8to15, _16to23, out min, out max);
            if (max >= 0) rtrn.Add(DayOfWeek.Saturday, new List<string>() { min.ToString(), max.ToString() });

            return rtrn;
        }

        private static void DecodeDay(byte _0to7, byte _8to15, byte _16to23, out int min, out int max)
        {
            var defaultAlgorithmOffset = 8;
            var currentOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now.ToLocalTime()).Hours;

            var periods = new byte[] { _0to7, _8to15, _16to23 };
            min = 24;
            max = -1;
            for (int p = 0; p < periods.Length; p++)
            {
                var period = periods[p];

                for (int i = 0; i < 8; i++)
                {
                    if ((period & Convert.ToInt32(Math.Pow(2, i))) > 0)
                    {
                        max = i + (p * 8) + (defaultAlgorithmOffset + currentOffset);
                        min = min > max ? max : min;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this.Connection != null)
            this.Connection.Dispose();
        }
    }
}
