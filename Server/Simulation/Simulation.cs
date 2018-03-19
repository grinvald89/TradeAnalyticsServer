using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Bollinger
    {
        public float Up;
        public float SMA;
        public float Down;

        public Bollinger(float Up, float SMA, float Down)
        {
            this.Up = Up;
            this.SMA = SMA;
            this.Down = Down;
        }

        public static float CalcSMA(List<Rate> Rates, int Index, int Period)
        {
            float sum = 0;

            for (int i = Index - Period + 1; i <= Index; i++)
                sum += Rates[i].Close;

            return sum / Period;
        }

        public static Bollinger Сalculate(List<Rate> Rates, int Index, int Period, float Deviations)
        {
            float StdDev;
            double sum = 0;

            for (int i = Index - Period + 1; i <= Index; i++)
                sum += Math.Pow(Rates[i].Close - CalcSMA(Rates, i, Period), 2);

            StdDev = Convert.ToSingle(Math.Sqrt(sum / Period));

            float SMA = CalcSMA(Rates, Index, Period);
            float Up = SMA + (Deviations * StdDev);
            float Down = SMA - (Deviations * StdDev);

            return new Bollinger(Up, SMA, Down);
        }
    }

    class Simulation
    {
        public static List<Bid> Start()
        {
            const int pairId = 12;
            const int timeFrame = 5;
            const int period = 13;
            const float deviations = (float)2.4;

            DateTime startTime = DateTime.Now;

            List<Pair> pairs = DataBase.DataBase.getPairs();

            Response response = new Response(pairId);

            List<Rate> rates = DataBase.DataBase.getRates(1000000, DateTime.Today, pairId, timeFrame, false);

            List<Bid> result = Analysis(rates, period, deviations);

            int success = result.FindAll(x => x.Success).Count;
            int count = result.Count;
            float percent = (float) success / result.Count * 100;

            result.Reverse();

            List<List<Bid>> resList = new List<List<Bid>>();

            for (int i = 20; i < 30; i++)
            {
                resList.Add(Analysis(rates, period, (float) i / 10));
            }

            float maxPercent = (float) resList[0].FindAll(x => x.Success).Count / resList[0].Count * 100;
            int index = 0;

            for (int i = 0; i < resList.Count; i++)
            {
                float a = (float)resList[i].FindAll(x => x.Success).Count / resList[i].Count * 100;

                if (a > maxPercent)
                {
                    maxPercent = a;
                    index = i;
                }
            }


            /*
            List<Bid> lastMonth = result.FindAll(x => x.Start.Date.Month == 2 && x.Start.Date.Year == 2018);

            int success_lastMonth = lastMonth.FindAll(x => x.Success).Count;
            float percent_lastMonth = (float) success_lastMonth / lastMonth.Count * 100;

            List<Bid> last3Month = result.FindAll(x => x.Start.Date.Month < 2 && x.Start.Date.Year == 2018);

            int success_last3Month = last3Month.FindAll(x => x.Success).Count;
            float percent_last3Month = (float)success_last3Month / last3Month.Count * 100;
            */

            return result;

            double time = (DateTime.Now - startTime).TotalSeconds;
        }

        static List<Bid> Analysis(List<Rate> Rates, int Period, float Deviations)
        {
            List<Bid> result = new List<Bid>();

            for (int i = 0; i < Rates.Count; i++)
            {
                if (i > Period * 2)
                {
                    Bollinger bollinger = Bollinger.Сalculate(Rates, i, Period, Deviations);

                    List<float> SMA = new List<float>();

                    for (int j = i - Period + 1; j <= i; j++)
                        SMA.Add(Bollinger.CalcSMA(Rates, j, Period));

                    float min = SMA[0];
                    float max = SMA[0];

                    foreach (var item in SMA)
                    {
                        if (item > max)
                            max = item;

                        if (item < min)
                            min = item;
                    }

                    if ((Rates[i].Close > bollinger.Up || Rates[i].Close < bollinger.Down) &&
                        (Rates[i - 1].Close < bollinger.Up && Rates[i - 1].Close > bollinger.Down) &&
                        (max - min < Math.Abs((Rates[i].Open - Rates[i].Close) * 2)) &&
                        Rates[i].Date.Hour > 9 &&
                        Rates[i].Date.DayOfWeek != DayOfWeek.Saturday &&
                        Rates[i].Date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        // 3, 6, 12, 15, 24
                        int time = 12;

                        result.Add(new Bid(
                            60,
                            Rates[i + 1],
                            Rates[i + 1 + time],
                            ((Rates[i + 1].Open - Rates[i + 1 + time].Close > 0) == Rates[i].Close > bollinger.Up)
                        ));
                    }
                }
            }

            return result;
        }
    }
}