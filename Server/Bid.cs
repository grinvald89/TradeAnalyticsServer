using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Bid
    {
        public int TimeFrame;
        public Tick Start;
        public Tick Finish;
        public bool Success;

        public Bid(int TimeFrame, Tick Start, Tick Finish, bool Success)
        {
            this.TimeFrame = TimeFrame;
            this.Start = Start;
            this.Finish = Finish;
            this.Success = Success;
        }
    }
}