using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Configuration;
using System.Data.Common;

namespace Makro.Windows.DesktopService.DataAccess
{
    public class SGIDataAccess : IDisposable
    {
        public DbConnection Connection { get; set; }

        //For tests only
        Random rnd = new Random();

        public SGIDataAccess(DbProviderFactory factory, string connectionString)
        {
            this.Connection = factory.CreateConnection();
            if (connectionString.ToLower().Contains("data source"))
            {
                this.Connection.ConnectionString = connectionString;
            }
            else
            {
                var namedCS = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
                this.Connection.ConnectionString = namedCS;
            }
        }

        public SGIDataAccess(DbConnection conn)
        {
            this.Connection = conn;
        }

        public bool IsUserLockable(string user)
        {
            var n = rnd.Next();
            //return n % 2 == 1;

            return true;
        }

        public void Dispose()
        {
            if (this.Connection != null)
            this.Connection.Dispose();
        }
    }
}
