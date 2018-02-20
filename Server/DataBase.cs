using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Server
{
    class DataBase
    {
        static SqlConnection sqlConnection = null;

        public static void open()
        {
            sqlConnection = new SqlConnection(Config.DBConfig);
            if (sqlConnection.State == ConnectionState.Closed)
                sqlConnection.Open();
        }

        public static void close()
        {
            sqlConnection.Close();
        }
    }
}
