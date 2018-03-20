using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Tick
    {
        public long Id;
        public long PairId;
        public float Value;
        public DateTime Date;

        public Tick(long PairId, float Value, DateTime Date)
        {
            this.PairId = PairId;
            this.Value = Value;
            this.Date = Date;
        }

        public Tick(long Id, long PairId, float Value, DateTime Date)
        {
            this.Id = Id;
            this.PairId = PairId;
            this.Value = Value;
            this.Date = Date;
        }
    }
}