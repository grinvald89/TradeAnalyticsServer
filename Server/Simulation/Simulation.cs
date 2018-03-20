using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Simulation
    {
        private static List<Tick> ticksOfLastCandlestick = new List<Tick>();
        private static List<Candlestick> candlesticks = new List<Candlestick>();

        const int pairId = 12;
        const int timeFrame = 5;
        const int bigPeriod = 13;
        const int smallPeriod = 4;

        public static List<Bid> Start()
        {
            List<Response> result = new List<Response>();

            List<Pair> pairs = DataBase.DataBase.getPairs();

            List<Tick> ticks = DataBase.DataBase.GetTicks(10000000, DateTime.Now, pairId);

            return Analysis(ticks);
        }


        private static List<Bid> Analysis(List<Tick> Ticks)
        {
            float percent;

            List<Bid> result = new List<Bid>();

            for (int i = 0; i < Ticks.Count; i++)
            {
                if (candlesticks.Count > bigPeriod)
                {
                    float currBigSMA = Indicators.SMA.CalcTicks(
                            candlesticks.GetRange(candlesticks.Count - bigPeriod - 1, bigPeriod),
                            Ticks[i]
                        );

                    float currSmallSMA = Indicators.SMA.CalcTicks(
                            candlesticks.GetRange(candlesticks.Count - smallPeriod - 1, smallPeriod),
                            Ticks[i]
                        );

                    float prevBigSMA = Indicators.SMA.CalcTicks(candlesticks.GetRange(candlesticks.Count - bigPeriod - 1, bigPeriod));

                    float prevSmallSMA = Indicators.SMA.CalcTicks(candlesticks.GetRange(candlesticks.Count - smallPeriod - 1, smallPeriod));

                    if ((prevBigSMA - prevSmallSMA > 0) != ((currBigSMA - currSmallSMA) > 0))
                    {
                        DateTime nextDate = Ticks[i].Date.AddMinutes(timeFrame);

                        Tick nextTick;

                        List<Tick> nextTicks = Ticks.FindAll(x => 
                            x.Date.Day == nextDate.Day &&
                            x.Date.Hour == nextDate.Hour &&
                            x.Date.Minute == nextDate.Minute
                        );

                        if (nextTicks.Count > 0)
                        {
                            nextTick = nextTicks[0];
                            int diffMillisecond = Math.Abs(Ticks[i].Date.Millisecond - nextTicks[0].Date.Millisecond);

                            foreach (Tick tick in nextTicks)
                                if (Math.Abs(tick.Date.Millisecond - Ticks[i].Date.Millisecond) < diffMillisecond)
                                {
                                    diffMillisecond = Math.Abs(tick.Date.Millisecond - Ticks[i].Date.Millisecond);
                                    nextTick = tick;
                                }

                            if ((result.Count == 0 || (Ticks[i].Date - result.Last().Finish.Date).Minutes >= timeFrame) && Ticks[i].Date.Hour > 8)
                            {
                                result.Add(new Bid(1,
                                    Ticks[i],
                                    nextTick,
                                    (Ticks[i].Value - nextTick.Value > 0) != (currBigSMA - currSmallSMA > 0))
                                );
                            }
                        }
                    }
                }

                AddTickToCandlesticks(Ticks[i]);

                percent = (float) result.FindAll(x => x.Success).Count / result.Count * 100;
            }

            return result;
        }


        private static void AddTickToCandlesticks(Tick Tick)
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

                if (DateEqual(candlesticks.Last().Date, Tick.Date) && Tick.Date.Minute % timeFrame == 0)
                    CalcLastCandlestick();

                ticksOfLastCandlestick.Add(Tick);
            }
            else
            {
                if (ticksOfLastCandlestick.Count > 0 && candlesticks.Count == 0 && ticksOfLastCandlestick.Last().Date.Second > Tick.Date.Second)
                    CalcLastCandlestick();

                ticksOfLastCandlestick.Add(Tick);
            }
        }

        private static void CalcLastCandlestick()
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
                1)
            );

            ticksOfLastCandlestick.Clear();
        }
    }
}