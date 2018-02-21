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


        public static List<Rate> getRates(int Take, DateTime FinishDate, long PairId, int Minutes)
        {
            if (sqlConnection.State == ConnectionState.Closed)
                open();

            List<Rate> lRates= new List<Rate>();

            using (var command = new SqlCommand(@"SELECT TOP (@Take) * FROM Rates WHERE Date < (@FinishDate) AND PairId = @PairId ORDER BY Date DESC", sqlConnection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter("Take", Take * Minutes),
                    new SqlParameter("FinishDate", FinishDate.ToString("yyyy-MM-ddTHH:mm:ss.000")),
                    new SqlParameter("PairId", PairId)
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
                            Convert.ToSingle(reader["Low"])
                        ));

                    lRates.AddRange(list);
                    reader.Close();
                }
            }

            if (lRates.Count == 1)
                return lRates;
            else
            {
                List<Rate> lResult = new List<Rate>();

                List<Rate> candlestick = new List<Rate>();

                for (int i = 0; i < lRates.Count + 1; i++)
                {
                    // Формируем свечу
                    if (candlestick.Count == Minutes)
                    {
                        float open = candlestick[candlestick.Count - 1].Open;
                        float close = candlestick[0].Close;

                        float high = candlestick[0].High;
                        float low = candlestick[0].Low;

                        foreach (Rate item in candlestick)
                        {
                            if (item.High > high)
                                high = item.High;

                            if (item.Low < low)
                                low = item.Low;
                        }

                        lResult.Add(new Rate(PairId, candlestick[0].Date, open, close, high, low));

                        candlestick.Clear();
                    }

                    if (i < lRates.Count)
                        candlestick.Add(lRates[i]);
                }

                return lResult;
            }
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