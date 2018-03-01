using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Config
    {
        public static string PATH = @"E:\Projects\TradeBot\Server\";

        public static string DBConfig = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\Projects\TradeBot\Server\Server\Trading.mdf;Integrated Security=True";
        //public static string DBConfig = "Database=grinvald9_trade;Data Source=mysql.grinvald9.myjino.ru;UID=grinvald9;password=bobahbes2008";   

        public static List<Pair> lPairs = new List<Pair>();

        public static int[] TimeFrames = { 1, 5, 10, 15, 30, 60};
    }
}