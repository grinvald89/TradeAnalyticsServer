using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Statisticscs
    {
        public int TimeFrame;
        public float Percent;
        public int Count;

        public Statisticscs(int TimeFrame, float Percent, int Count)
        {
            this.TimeFrame = TimeFrame;
            this.Percent = Percent;
            this.Count = Count;
        }
    }

    class Bid
    {
        public int TimeFrame;
        public Rate Start;
        public Rate Finish;
        public bool Success;

        public Bid(int TimeFrame, Rate Start, Rate Finish, bool Success)
        {
            this.TimeFrame = TimeFrame;
            this.Start = Start;
            this.Finish = Finish;
            this.Success = Success;
        }
    }

    class Mounth
    {
        public int Number;
        public List<Statisticscs> Statistics;

        public Mounth(int Number)
        {
            this.Number = Number;
        }
    }

    class Response
    {
        public long PairId;
        public List<Bid> Bids;

        public Mounth[] Mounth;
        public Statisticscs Overall;

        public Response(long PairId)
        {
            this.PairId = PairId;
        }
    }
}