using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Strategies
    {
        #region Пересечение скользящих средних
        public static bool IntersectionSMA(int CurrentTickIndex, List<Tick> Ticks, List<Candlestick> Candlesticks, int TimeFrame, int BigPeriod, int SmallPeriod)
        {
            float currBigSMA = Indicators.SMA.CalcTicks(
                Candlesticks.GetRange(Candlesticks.Count - BigPeriod, BigPeriod),
                Ticks[CurrentTickIndex]
            );

            float currSmallSMA = Indicators.SMA.CalcTicks(
                Candlesticks.GetRange(Candlesticks.Count - SmallPeriod, SmallPeriod),
                Ticks[CurrentTickIndex]
            );

            float prevBigSMA = Indicators.SMA.CalcTicks(Candlesticks.GetRange(Candlesticks.Count - BigPeriod, BigPeriod));

            float prevSmallSMA = Indicators.SMA.CalcTicks(Candlesticks.GetRange(Candlesticks.Count - SmallPeriod, SmallPeriod));

            if ((prevBigSMA - prevSmallSMA > 0) != ((currBigSMA - currSmallSMA) > 0) && Ticks[CurrentTickIndex].Date.Hour > 5)
            {
                Tick finishTick = GetFinishTick(CurrentTickIndex, Ticks, TimeFrame);

                if (finishTick != null)
                    return ((Ticks[CurrentTickIndex].Value - finishTick.Value > 0) != (currBigSMA - currSmallSMA > 0));
            }

            return true;
        }

        private static Tick GetFinishTick(int CurrentTickIndex, List<Tick> Ticks, int TimeFrame)
        {
            DateTime time = Ticks[CurrentTickIndex].Date.AddMinutes(TimeFrame);
            time.AddSeconds(Ticks[CurrentTickIndex].Date.Second);

            List<Tick> finishTicks = new List<Tick>();

            for (int j = CurrentTickIndex + 1; j < Ticks.Count - 1 && Ticks[j].Date.CompareTo(time) < 0; j++)
                if (Ticks[j].Date.Day == time.Day &&
                    Ticks[j].Date.Hour == time.Hour &&
                    Ticks[j].Date.Minute == time.Minute)
                        finishTicks.Add(Ticks[j]);

            return (finishTicks.Count > 0) ? finishTicks.Last() : null;
        }

        #endregion
    }
}