using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Result
    {
        public int BigSMAPeriod;
        public int SmallSMAPeriod;
        public float Percent;
        public int Count;

        public Result(int BigSMAPeriod, int SmallSMAPeriod, float Percent, int Count)
        {
            this.BigSMAPeriod = BigSMAPeriod;
            this.SmallSMAPeriod = SmallSMAPeriod;
            this.Percent = Percent;
            this.Count = Count;
        }
    }

    class Simulation
    {
        private static List<Tick> ticksOfLastCandlestick = new List<Tick>();
        private static List<Candlestick> candlesticks = new List<Candlestick>();

        const int pairId = 12;

        public static List<Bid> Start()
        {
            List<Tick> ticks = DataBase.DataBase.GetTicks("OlympTradeTicks", 10000000, DateTime.Now, pairId);

            Tick _tick = ticks.First();

            List<int> indexes = new List<int>();

            for (int i = 1; i < ticks.Count; i++)
            {
                if (_tick.Date.Year == ticks[i].Date.Year &&
                    _tick.Date.Month == ticks[i].Date.Month &&
                    _tick.Date.Day == ticks[i].Date.Day &&
                    _tick.Date.Hour == ticks[i].Date.Hour &&
                    _tick.Date.Minute == ticks[i].Date.Minute &&
                    _tick.Date.Second == ticks[i].Date.Second)
                {
                    if (_tick.Value == ticks[i].Value)
                        indexes.Add(i);
                    else
                        _tick = ticks[i];
                }
                else
                    _tick = ticks[i];
            }

            for (int i = indexes.Count - 1; i >= 0; i--)
                ticks.RemoveAt(indexes[i]);

            List<Result> results = new List<Result>();

            for (int i = 12; i <= 60; i++)
            {
                for (int j = 2; j <= 10; j++)
                {
                    List<Bid> res = Analysis(ticks, i, j, 1);

                    results.Add(new Result(i, j, (float) res.FindAll(x => x.Success).Count / res.Count * 100, res.Count));
                }
            }

            List<Result> results1 = results.OrderByDescending(x => x.Percent).ToList();

            int bidsSuccess = 0;
            int bidsFails = 0;
            int bidsInSuccess = 0;

            foreach (Result _result in results)
            {
                if (_result.Percent >= 57)
                    bidsSuccess += _result.Count;

                if (_result.Percent < 57 && _result.Percent > 43)
                    bidsFails += _result.Count;

                if (_result.Percent <= 43)
                    bidsInSuccess += _result.Count;
            }

            return Analysis(ticks, 13, 4, 1);
        }


        private static List<Bid> Analysis(List<Tick> Ticks, int BigSMAPeriod, int SmallSMAPeriod, int TimeFrame)
        {
            List<Bid> result = new List<Bid>();

            /*for (int i = 0; i < Ticks.Count; i++)
            {
                if (Candlesticks.Count > BigSMAPeriod)
                    Strategies.IntersectionSMA();

                if ((result.Count == 0 || (Ticks[CurrentTickIndex].Date - result.Last().Finish.Date).Minutes >= TimeFrame)) {
                    result.Add(new Bid(1,
                        Ticks[CurrentTickIndex],
                        nextTicks.Last(),
                        (Ticks[CurrentTickIndex].Value - nextTicks.Last().Value > 0) != (currBigSMA - currSmallSMA > 0))
                    );
                }
                
                AddTickToCandlesticks(Ticks[i], TimeFrame);
            }*/

            return result;
        }


        private static void AddTickToCandlesticks(Tick Tick, int TimeFrame)
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

                if (DateEqual(candlesticks.Last().Date, Tick.Date) && Tick.Date.Minute % TimeFrame == 0)
                    CalcLastCandlestick(TimeFrame);

                ticksOfLastCandlestick.Add(Tick);
            }
            else
            {
                if (ticksOfLastCandlestick.Count > 0 && candlesticks.Count == 0 && ticksOfLastCandlestick.Last().Date.Second > Tick.Date.Second)
                    CalcLastCandlestick(TimeFrame);

                ticksOfLastCandlestick.Add(Tick);
            }
        }

        private static void CalcLastCandlestick(int TimeFrame)
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
                TimeFrame)
            );

            ticksOfLastCandlestick.Clear();
        }
    }
}