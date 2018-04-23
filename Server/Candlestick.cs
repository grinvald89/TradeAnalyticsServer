using System;
using System.Collections.Generic;

namespace Server
{
    class Candlestick
    {
        public long PairId;
        public DateTime Date;
        public float Open;
        public float Close;
        public float High;
        public float Low;
        public int TimeFrame;

        // public Candlestick(long PairId, DateTime Date, float Open, float Close, float High, float Low, int TimeFrame)
        public Candlestick(long PairId, DateTime Date, float Open, float High, float Low, float Close, int TimeFrame)
        {
            this.PairId = PairId;
            this.Date = Date;
            this.Open = Open;
            this.Close = Close;
            this.High = High;
            this.Low = Low;
            this.TimeFrame = TimeFrame;
        }

        public static float GetShadowToBody(Candlestick Candlestick)
        {
            if (Math.Abs(Candlestick.Open - Candlestick.Close) == 0)
                return 0;
            else
                return (float) (Candlestick.High - Candlestick.Low) / Math.Abs(Candlestick.Open - Candlestick.Close);
        }

        public static float GetBody(Candlestick Candlestick)
        {
            return Math.Abs(Candlestick.Open - Candlestick.Close);
        }

        public static float GetShadow(Candlestick Candlestick)
        {
            return Math.Abs(Candlestick.High - Candlestick.Low);
        }

        public static float GetMeanBodyOfList(List<Candlestick> Candlesticks)
        {
            float sum = 0;

            foreach (Candlestick candlestick in Candlesticks)
                sum += Math.Abs(candlestick.Open - candlestick.Close);

            return (float) sum / Candlesticks.Count;
        }

        public static float GetMeanShadowOfList(List<Candlestick> Candlesticks)
        {
            float sum = 0;

            foreach (Candlestick candlestick in Candlesticks)
                sum += Math.Abs(candlestick.High - candlestick.Low);

            return (float) sum / Candlesticks.Count;
        }
    }
}