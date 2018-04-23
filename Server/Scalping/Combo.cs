using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Scalping
{
    class Combo
    {
        public Candlestick One;
        public Candlestick Two;
        public int TurnTime;

        public Combo(Candlestick One, Candlestick Two, int TurnTime)
        {
            this.One = One;
            this.Two = Two;
            this.TurnTime = TurnTime;
        }
    }
}