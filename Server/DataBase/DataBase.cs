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


        // IsReverseDate наверно вынести в другой запрос
        public static List<Rate> getRates(int Take, DateTime Date, long PairId, int TimeFrame, bool IsForward)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            List<Rate> lRates= new List<Rate>();

            string sOperator = "<";
            string sSortDESC= "DESC";

            if (IsForward)
                sOperator = ">";

            if (IsForward)
                sSortDESC = "ASC";

            using (var command = new SqlCommand(@"SELECT TOP (@Take) * FROM Rates WHERE Date " + sOperator + @" (@Date) AND PairId = @PairId AND TimeFrame = @TimeFrame ORDER BY Date " + sSortDESC, sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("Take", Take),
                    new SqlParameter("Date", Date.ToString("yyyy-MM-ddTHH:mm:ss.000")),
                    new SqlParameter("PairId", PairId),
                    new SqlParameter("TimeFrame", TimeFrame)
                });

                using (var reader = command.ExecuteReader())
                {
                    var list = new List<Rate>();
                    while (reader.Read())
                        list.Add(new Rate(
                            Convert.ToInt64(reader["Id"]),
                            Convert.ToDateTime(reader["Date"]),
                            Convert.ToSingle(reader["Open"]),
                            Convert.ToSingle(reader["Close"]),
                            Convert.ToSingle(reader["High"]),
                            Convert.ToSingle(reader["Low"]),
                            Convert.ToInt32(reader["TimeFrame"])
                        ));

                    lRates.AddRange(list);
                    reader.Close();
                }
            }

            if (!IsForward)
                lRates.Reverse();

            return lRates;
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

            using (var command = new SqlCommand("INSERT INTO [Rates] (PairId, Date, [Open], [Close], High, Low, TimeFrame)VALUES(@PairId, @Date, @Open, @Close, @High, @Low, @TimeFrame)", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("PairId", rate.PairId),
                    new SqlParameter("Date", rate.Date),
                    new SqlParameter("Open", rate.Open),
                    new SqlParameter("Close", rate.Close),
                    new SqlParameter("High", rate.High),
                    new SqlParameter("Low", rate.Low),
                    new SqlParameter("TimeFrame", rate.TimeFrame)
                });

                using (var reader = command.ExecuteReader())
                    reader.Close();
            }
        }
    }
}