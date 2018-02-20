using System;

namespace Server
{
    class Rate
    {
        public long PairId;
        public DateTime Date;
        public float Open;
        public float Close;
        public float High;
        public float Low;

        public Rate(long PairId, DateTime Date, float Open, float Close, float High, float Low)
        {
            this.PairId = PairId;
            this.Date = Date;
            this.Open = Open;
            this.Close = Close;
            this.High = High;
            this.Low = Low;
        }
    }
}