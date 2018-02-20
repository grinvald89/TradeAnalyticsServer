using System;
using System.Text;
using System.Net.Sockets;
using System.Web.Script.Serialization;

namespace Server.WebServer
{
    class Client
    {
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            string Request = ""; // Объявим строку, в которой будет хранится запрос клиента
            byte[] Buffer = new byte[1024]; // Буфер для хранения принятых от клиента данных
            int Count; // Переменная для хранения количества байт, принятых от клиента

            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);

                if (Request.IndexOf("}") >= 0 || Request.Length > 4096)
                    break;
            }

            //if (Request.IndexOf(@"POST /api/writeRate/") != -1)
            //    addRate(getJsonData(Request));

            // Код простой HTML-странички
            string Html = "<html><body><h1>It works!</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Bufferr = Encoding.ASCII.GetBytes(Str);

            // Отправим его клиенту
            Client.GetStream().Write(Bufferr, 0, Bufferr.Length);
            // Закроем соединение
            Client.Close();
        }
    }
}
