using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SQLite;

namespace RMS
{
    public class SQLiteHandler
    {

        public System.Data.SQLite.SQLiteConnection getConnection()
        {
            string cs = System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

            return new SQLiteConnection(cs);

        }
    }
}
