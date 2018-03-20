using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Indicators
{
    class SMA
    {
        public static float CalcTicks(List<Candlestick> Candlesticks)
        {
            float sum = 0;

            foreach (var candlestick in Candlesticks)
                sum += candlestick.Close;

            return sum / Candlesticks.Count;
        }

        public static float CalcTicks(List<Candlestick> Candlesticks, Tick LastTick)
        {
            float sum = 0;

            foreach (var candlestick in Candlesticks)
                sum += candlestick.Close;

            return (sum + LastTick.Value) / (Candlesticks.Count + 1);
        }
    }
}