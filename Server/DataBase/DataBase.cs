using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Server.DataBase
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


        // Получить все валютные пары
        public static List<Pair> getPairs()
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            List<Pair> lPairs = new List<Pair>();

            using (var command = new SqlCommand("SELECT * FROM Pairs", sqlConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var list = new List<Pair>();
                    while (reader.Read())
                        list.Add(new Pair(
                            Convert.ToInt64(reader["Id"]),
                            Convert.ToString(reader["Name"])
                        ));

                    lPairs.AddRange(list);
                    reader.Close();
                }
            }

            return lPairs;
        }


        // Добавляем валютную пару
        public static long addPair(string Name)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            using (var command = new SqlCommand("INSERT INTO [Pairs] (Name) OUTPUT INSERTED.ID VALUES(@Name)", sqlConnection))
            {
                command.Parameters.AddWithValue("Name", Name);

                return (int)command.ExecuteScalar();
            }
        }


        // Добавляем Rate
        public static void addRate(Rate rate)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            using (var command = new SqlCommand("INSERT INTO [Rates] (PairId, Date, [Open], [Close], High, Low)VALUES(@PairId, @Date, @Open, @Close, @High, @Low)", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                        new SqlParameter("PairId", rate.PairId),
                        new SqlParameter("Date", rate.Date),
                        new SqlParameter("Open", rate.Open),
                        new SqlParameter("Close", rate.Close),
                        new SqlParameter("High", rate.High),
                        new SqlParameter("Low", rate.Low)
                });

                using (var reader = command.ExecuteReader())
                    reader.Close();
            }
        }
    }
}
