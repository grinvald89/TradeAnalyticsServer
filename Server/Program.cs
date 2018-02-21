using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Server
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DataBase.DataBase.open();
            Config.lPairs = DataBase.DataBase.getPairs();

            // DataBase.TXTtoDB.start();

            // List<Rate> r = DataBase.DataBase.getRates(10, new DateTime(2018,2,16,0,0,0), 1, 5);

            ThreadPool.QueueUserWorkItem(StateInfo => new WebServer.Server(8080));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}