using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
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
            int startIndex = Url.IndexOf("?") + 1;
            int finishIndex = Url.IndexOf(" ", Url.IndexOf(" ") + 1) + 1 - startIndex;

            string sParams = Url.Substring(startIndex, finishIndex);
            bool isForward = false;

            DateTime date = DateTime.Today;

            if (Url.IndexOf("date") != -1)
                date = Convert.ToDateTime(GetParam("date", sParams.Split('&')));

            if (Url.IndexOf("isforward") != -1)
                isForward = true;

            List<Rate> lResult = DataBase.DataBase.getRates(
                Convert.ToInt32(GetParam("take", sParams.Split('&'))),
                date,
                Convert.ToInt64(GetParam("pairid", sParams.Split('&'))),
                Convert.ToInt32(GetParam("timeframe", sParams.Split('&'))),
                isForward
            );

            List<ResponseRate> Result = new List<ResponseRate>();

            foreach (var item in lResult)
                Result.Add(new ResponseRate(item.PairId, item.Date.ToString(), item.Open, item.Close, item.High, item.Low));

            return new JavaScriptSerializer().Serialize(Result);
        }

        static string GetParam(string Name, string[] Params)
        {
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