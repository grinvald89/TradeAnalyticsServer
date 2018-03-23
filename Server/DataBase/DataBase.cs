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
        public static List<Candlestick> getRates(int Take, DateTime Date, long PairId, int TimeFrame, bool IsForward)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            List<Candlestick> lRates= new List<Candlestick>();

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
                    var list = new List<Candlestick>();
                    while (reader.Read())
                        list.Add(new Candlestick(
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
        public static void addRate(Candlestick rate)
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


        public static List<Tick> GetTicks(string Table, int Take, DateTime Date, long PairId)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            List<Tick> result = new List<Tick>();

            using (var command = new SqlCommand(@"SELECT TOP (@Take) * FROM " + Table + @" WHERE Date < (@Date) AND PairId = @PairId ORDER BY Date DESC", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("Take", Take),
                    new SqlParameter("Date", Date.ToString("yyyy-MM-ddTHH:mm:ss.000")),
                    new SqlParameter("PairId", PairId)
                });

                using (var reader = command.ExecuteReader())
                {
                    var list = new List<Tick>();
                    while (reader.Read())
                        list.Add(new Tick(
                            Convert.ToInt64(reader["Id"]),
                            Convert.ToInt64(reader["PairId"]),
                            Convert.ToSingle(reader["Value"]),
                            Convert.ToDateTime(reader["Date"])
                        ));

                    result.AddRange(list);
                    reader.Close();
                }
            }

            result.Reverse();

            return result;
        }

        public static void AddTick(Tick Tick)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            using (var command = new SqlCommand("INSERT INTO [Ticks] (PairId, Value, Date)VALUES(@PairId, @Value, @Date)", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("PairId", Tick.PairId),
                    new SqlParameter("Value", Tick.Value),
                    new SqlParameter("Date", Tick.Date)
                });

                using (var reader = command.ExecuteReader())
                    reader.Close();
            }
        }


        public static void AddTickOlympTrade(Tick Tick, int Percent)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            using (var command = new SqlCommand("INSERT INTO [OlympTradeTicks] (PairId, Value, Date, [Percent])VALUES(@PairId, @Value, @Date, @Percent)", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("PairId", Tick.PairId),
                    new SqlParameter("Value", Tick.Value),
                    new SqlParameter("Date", Tick.Date),
                    new SqlParameter("Percent", Percent)
                });

                using (var reader = command.ExecuteReader())
                    reader.Close();
            }
        }
    }
}