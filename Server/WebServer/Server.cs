using System.Net;
using System.Net.Sockets;

namespace Server.WebServer
{
    class Server
    {
        // Объект, принимающий TCP-клиентов
        TcpListener Listener;

        // Запуск сервера
        public Server(int Port)
        {
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start(); // Запускаем его

            Listener.AcceptSocket();

            while (true)
                new Client(Listener.AcceptTcpClient());
        }

        // Остановка сервера
        ~Server()
        {
            if (Listener != null)
                Listener.Stop();

            DataBase.close();
        }
    }
}
