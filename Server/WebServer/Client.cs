using System;
using System.Text;
using System.Net.Sockets;
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
                string data = "{data: 1, dete: 2}";
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

            //if (Request.IndexOf(@"POST /api/writeRate/") != -1)
            //    addRate(getJsonData(Request));
        }
    }
}