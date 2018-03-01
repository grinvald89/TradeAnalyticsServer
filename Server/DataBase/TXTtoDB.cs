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
        public static void Start()
        {
            int finishedFiles = 0;

            string[] files = Directory.GetFiles(Config.PATH + @"\Rates", "*.txt");

            foreach (string file in files)
            {
                int iTimeFrame = file.LastIndexOf(Regex.Match(file, @"-[0-9]{1,2}.").ToString());
                int timeFrame = 1;

                if (iTimeFrame != 1)
                    timeFrame = Convert.ToInt32(file.Substring(iTimeFrame + 1).Replace(".txt", ""));

                StreamReader objReader = new StreamReader(Config.PATH + @"\Rates\" + file.Substring(file.LastIndexOf(@"\") + 1));
                string sLine = "";
                ArrayList textFile = new ArrayList();

                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                        textFile.Add(sLine);
                }
                objReader.Close();

                WriteToBD(textFile, timeFrame, finishedFiles);

                finishedFiles++;
            }
        }


        static void WriteToBD(ArrayList TextFile, int TimeFrame, int finishedFiles)
        {
            foreach (string sOutput in TextFile)
            {
                DateTime Date = GetDate(Regex.Match(sOutput, @";[0-9\/]{8};").ToString().Replace(";", ""), Regex.Match(sOutput, @";[0-9\:]{8};").ToString().Replace(";", ""));

                string _sOutput = sOutput.Substring(sOutput.IndexOf(Regex.Match(sOutput, @";[0-9\:]{8};").ToString()) + 10);
                float Open = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float High = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float Low = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                _sOutput = _sOutput.Substring(_sOutput.IndexOf(";") + 1);
                float Close = Convert.ToSingle(Regex.Match(_sOutput, @"^[0-9\.]+;").ToString().Replace(";", "").Replace(".", ","));

                DataBase.addRate(new Rate(
                    GetPairId(Regex.Match(sOutput, @"^[A-Z]+;").ToString().Replace(";", "")),
                    GetDate(Regex.Match(sOutput, @";[0-9\/]{8};").ToString().Replace(";", ""), Regex.Match(sOutput, @";[0-9\:]{8};").ToString().Replace(";", "")),
                    Open,
                    Close,
                    High,
                    Low,
                    TimeFrame
                ));
            }
        }


        static long GetPairId(string Name)
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

        static DateTime GetDate(string Date, string Time)
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