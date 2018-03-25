using System;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Server.WebServer
{
    class Client
    {
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient client)
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];  // Буфер для хранения принятых от клиента данных
            StringBuilder request = new StringBuilder(); // Объявим строку, в которой будет хранится запрос клиента
            NetworkStream stream = client.GetStream(); // Сохраним поток

            if (stream.CanRead)
            {
                do
                {
                    int bytes = stream.Read(buffer, 0, (int)client.ReceiveBufferSize);
                    request.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
                }
                while (stream.DataAvailable); // пока данные есть в потоке
            }

            if (stream.CanWrite)
            {

                string data = Routing(request.ToString());
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                string sResponse = "HTTP/1.1 200 OK\nContent-type: application/json\nAccess-Control-Allow-Origin: *\nContent-Length:" + data.Length.ToString() + "\n\n" + data;
                // Приведем строку к виду массива байт
                byte[] bResponse = Encoding.ASCII.GetBytes(sResponse);

                // Отправим его клиенту
                stream.Write(bResponse, 0, bResponse.Length);
            }

            // Закроем поток
            stream.Close();

            // Закроем соединение
            client.Close();
        }


        static string Routing(string Url)
        {
            if (Url.IndexOf(@"GET /api/getRates/?") != -1)
                return GetRates(Url);

            if (Url.IndexOf(@"GET /api/getPairs/") != -1)
                return GetPairs();

            if (Url.IndexOf(@"GET /api/addTick/") != -1)
                return AddTick(Url);

            return String.Empty;
        }


        static string AddTick(string Url)
        {
            try
            {
                long pairId = Convert.ToInt64(GetParam("PairId", Url));
                float value = Convert.ToSingle(GetParam("Value", Url).Replace(".", ","));
                string sDate = GetParam("Date", Url).Replace("%20", " ");
                int percent = Convert.ToInt32(GetParam("Percent", Url));

                int dDay = Convert.ToInt32(Regex.Match(sDate, @"^[0-9]{1,2}\.").ToString().Replace(".", ""));
                int dMonth = Convert.ToInt32(Regex.Match(sDate, @"\.[0-9]{2}\.").ToString().Replace(".", ""));
                int dYear = Convert.ToInt32(Regex.Match(sDate, @"\.[0-9]{4}\s").ToString().Replace(".", "").Replace(" ", ""));
                int dHour = Convert.ToInt32(Regex.Match(sDate, @"\s[0-9]{2}\:").ToString().Replace(":", "").Replace(" ", ""));
                int dMinute = Convert.ToInt32(Regex.Match(sDate, @"\:[0-9]{2}\:").ToString().Replace(":", ""));
                int dSeconds = Convert.ToInt32(sDate.Substring(sDate.Length - 2));

                Tick tick = new Tick(pairId, value, new DateTime(dYear, dMonth, dDay, dHour, dMinute, dSeconds));

                DataBase.DataBase.AddTickOlympTrade(tick, percent);
            }
            catch { }

            return String.Empty;
        }


        class ResponseRate
        {
            public long PairId;
            public string Date;
            public float Open;
            public float Close;
            public float High;
            public float Low;

            public ResponseRate(long PairId, string Date, float Open, float Close, float High, float Low)
            {
                this.PairId = PairId;
                this.Date = Date;
                this.Open = Open;
                this.Close = Close;
                this.High = High;
                this.Low = Low;
            }
        }

        static string GetRates(string Url)
        {
            /*
            bool isForward = false;

            DateTime date = DateTime.Today;

            if (Url.IndexOf("date") != -1)
                date = Convert.ToDateTime(GetParam("date", Url));

            if (Url.IndexOf("isforward") != -1)
                isForward = true;

            List<Candlestick> lResult = DataBase.DataBase.getRates(
                Convert.ToInt32(GetParam("take", Url)),
                date,
                Convert.ToInt64(GetParam("pairid", Url)),
                Convert.ToInt32(GetParam("timeframe", Url)),
                isForward
            );
            */

            candlesticks.Clear();

            List<Tick> ticks = DataBase.DataBase.GetTicks("OlympTradeTicks", 10000000, DateTime.Now, 12);

            foreach (Tick tick in ticks)
                AddTickToCandlesticks(tick);



            List<ResponseRate> Result = new List<ResponseRate>();

            foreach (var item in candlesticks)
                Result.Add(new ResponseRate(item.PairId, item.Date.ToString(), item.Open, item.Close, item.High, item.Low));

            return new JavaScriptSerializer().Serialize(Result);
        }

        #region FormCandlestick
        private static List<Tick> ticksOfLastCandlestick = new List<Tick>();
        private static List<Candlestick> candlesticks = new List<Candlestick>();

        const int timeFrame = 1;

        private static void AddTickToCandlesticks(Tick Tick)
        {
            if (candlesticks.Count > 0)
            {
                bool DateEqual(DateTime PrevDate, DateTime CurDate)
                {
                    if (PrevDate.Year != CurDate.Year ||
                        PrevDate.Month != CurDate.Month ||
                        PrevDate.Day != CurDate.Day ||
                        PrevDate.Hour != CurDate.Hour ||
                        PrevDate.Minute != CurDate.Minute)
                        return true;
                    else
                        return false;
                }

                if (DateEqual(candlesticks.Last().Date, Tick.Date) && Tick.Date.Minute % timeFrame == 0)
                    CalcLastCandlestick();

                ticksOfLastCandlestick.Add(Tick);
            }
            else
            {
                if (ticksOfLastCandlestick.Count > 0 && candlesticks.Count == 0 && ticksOfLastCandlestick.Last().Date.Second > Tick.Date.Second)
                    CalcLastCandlestick();

                ticksOfLastCandlestick.Add(Tick);
            }
        }


        private static void CalcLastCandlestick()
        {
            float high = ticksOfLastCandlestick[0].Value;
            float low = ticksOfLastCandlestick[0].Value;

            foreach (Tick tick in ticksOfLastCandlestick)
            {
                if (tick.Value > high)
                    high = tick.Value;

                if (tick.Value < low)
                    low = tick.Value;
            }

            candlesticks.Add(new Candlestick(
                ticksOfLastCandlestick[0].PairId,
                ticksOfLastCandlestick[0].Date,
                ticksOfLastCandlestick[0].Value,
                ticksOfLastCandlestick.Last().Value,
                high,
                low,
                timeFrame)
            );

            ticksOfLastCandlestick.Clear();
        }
        #endregion

        static string GetParam(string Name, string Url)
        {
            int startIndex = Url.IndexOf("?") + 1;
            int finishIndex = Url.IndexOf(" ", Url.IndexOf(" ") + 1) + 1 - startIndex;

            string sParams = Url.Substring(startIndex, finishIndex);
            string[] Params = sParams.Split('&');

            string result = String.Empty;

            foreach (string item in Params)
                if (item.IndexOf(Name) != -1)
                    result = item;

            result = result.Substring(result.IndexOf("=") + 1);

            return result;
        }

        static string GetPairs()
        {
            return new JavaScriptSerializer().Serialize(DataBase.DataBase.getPairs());
        }
    }
}