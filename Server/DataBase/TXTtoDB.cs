using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Server.DataBase
{
    class TXTtoDB
    {
        public static void start()
        {
            StreamReader objReader = new StreamReader(Config.PATH + "input.txt");
            string sLine = "";
            ArrayList arrText = new ArrayList();

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    arrText.Add(sLine);
            }
            objReader.Close();

            foreach (string sOutput in arrText)
            {
                DateTime Date = getDate(Regex.Match(sOutput, @";[0-9\/]{8};").ToString().Replace(";", ""), Regex.Match(sOutput, @";[0-9\:]{8};").ToString().Replace(";", ""));

                string _sOutput = sOutput.Substring(sOutput.IndexOf(Regex.Match(sOutput, @";[0-9\:]{8};").ToString()) + 10);
                float Open = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float High = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float Low = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float Close = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                DataBase.addRate(new Rate(
                    getPairId(Regex.Match(sOutput, @"^[A-Z]+;").ToString().Replace(";", "")),
                    getDate(Regex.Match(sOutput, @";[0-9\/]{8};").ToString().Replace(";", ""), Regex.Match(sOutput, @";[0-9\:]{8};").ToString().Replace(";", "")),
                    Open,
                    Close,
                    High,
                    Low
                ));
            }
        }

        static long getPairId(string Name)
        {
            if (Config.lPairs.Find(x => x.Name == Name) == null)
            {
                long Id = DataBase.addPair(Name);
                Config.lPairs.Add(new Pair(Id, Name));

                return Id;
            }
            else
                return Config.lPairs.Find(x => x.Name == Name).Id;
        }

        static DateTime getDate(string Date, string Time)
        {
            int Year = Convert.ToInt32(("20" + Regex.Match(Date, @"/[0-9]{2}$")).ToString().Replace(@"/", ""));
            int Mounth = Convert.ToInt32(Regex.Match(Date, @"/[0-9]{2}/").ToString().Replace(@"/", ""));
            int Day = Convert.ToInt32(Regex.Match(Date, @"^[0-9]{2}/").ToString().Replace(@"/", ""));

            int Hour = Convert.ToInt32(Regex.Match(Time, @"^[0-9]{2}:").ToString().Replace(@":", ""));
            int Minute = Convert.ToInt32(Regex.Match(Time, @":[0-9]{2}:").ToString().Replace(@":", ""));
            int Second = Convert.ToInt32(Regex.Match(Time, @":[0-9]{2}$").ToString().Replace(@":", ""));

            return new DateTime(Year, Mounth, Day, Hour, Minute, Second);
        }
    }
}