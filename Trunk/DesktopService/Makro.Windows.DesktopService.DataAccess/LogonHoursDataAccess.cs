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
            try
            {
                bytes = GetUserLogonHours(user);
            }
            catch (Exception ex)
            {
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
                return false;


            return false;
        }

        private byte[] GetUserLogonHours(string user)
        {
            user = "hnogueira";

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
            var currentOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now.ToLocalTime()).Hours;

            var str = new string[168];

            //if (currentOffset < 0)
            //{
            //    str = str.Substring(str.Length - Math.Abs(currentOffset)) + str.Substring(0, str.Length - Math.Abs(currentOffset));
            //} 
            //else if (currentOffset > 0)
            //{
            //    str = str.Substring(Math.Abs(currentOffset)) + str.Substring(0, Math.Abs(currentOffset));
            //}

            // # Loop through the 21 bytes in the array, each representing 8 hours.
            for (var j = 0; j <= 20; j++)
            {
                // Check each of the 8 bits in each byte.
                for (var k = 7; k >= 0; k--)
                {
                    // Adjust the index into an array of hours for the
                    // local time zone bias.
                    var m = (8 * j) + (k + currentOffset);
                    // The index into the  array of hours ranges from 0 to 167.
                    if (m < 0) { m += 168; }
                    else if (m > 167) { m -= 168; }

                    // Check the bit of the byte and assign the corresponding
                    // element of the array.
                    if ((bytes[j] & (int)Math.Pow(2, k)) > 0) { str[m] = "1"; }
                    else { str[m] = "0"; }
                }
            }

            var rtrn = new Dictionary<DayOfWeek, List<String>>();
            int? min;
            int? max;

            //Sunday
            DecodeDay(String.Concat(str.Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Sunday, new List<string>() { min.ToString(), max.ToString() });

            //Monday
            DecodeDay(String.Concat(str.Skip(24).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Monday, new List<string>() { min.ToString(), max.ToString() });

            //Tuesday
            DecodeDay(String.Concat(str.Skip(48).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Tuesday, new List<string>() { min.ToString(), max.ToString() });

            //Wednesday
            DecodeDay(String.Concat(str.Skip(72).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Wednesday, new List<string>() { min.ToString(), max.ToString() });

            //Thursday
            DecodeDay(String.Concat(str.Skip(96).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Thursday, new List<string>() { min.ToString(), max.ToString() });

            //Friday
            DecodeDay(String.Concat(str.Skip(120).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Friday, new List<string>() { min.ToString(), max.ToString() });

            //Saturday
            DecodeDay(String.Concat(str.Skip(144).Take(24)), out min, out max);
            if (max.HasValue) rtrn.Add(DayOfWeek.Saturday, new List<string>() { min.ToString(), max.ToString() });

            return rtrn;
        }

        private void DecodeDay(string dayString, out int? min, out int? max)
        {
            max = null;
            min = null;
            for (int i = 0; i < dayString.Length; i++)
            {
                if (dayString[i] == '1')
                {
                    max = i >= max.GetValueOrDefault() ? i : max;
                    if (!min.HasValue) min = i;
                }

            }
        }

        private static string ToBitString(byte[] bytes)
        {
            var str = bytes.Aggregate("", (s, b) =>
            {
                s += Convert.ToString(b, 2).PadLeft(8, '0');
                return s;
            });
            return str;
        }

        [Obsolete]
        private static void DecodeDay(byte _0to7, byte _8to15, byte _16to23, out int min, out int max)
        {
            var currentOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now.ToLocalTime()).Hours;
            var periods = new byte[] { _0to7, _8to15, _16to23 };
            min = 24;
            max = -1;
            for (int p = 0; p < periods.Length; p++)
            {
                var period = periods[p];
                var offset = Math.Abs(currentOffset);
                if (currentOffset < 0)
                {
                    var tmp = (byte)(period << offset);
                    tmp += (byte)(period >> 8 - offset);
                    period = tmp;
                }

                if (currentOffset > 0)
                    period = (byte)(period >> Math.Abs(currentOffset));

                for (int i = 0; i < 8; i++)
                {
                    if ((period & Convert.ToInt32(Math.Pow(2, i))) > 0)
                    {
                        max = i + (p * 8);
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
