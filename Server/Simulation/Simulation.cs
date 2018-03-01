using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Simulation
    {
        public static List<Response> Start()
        {
            DateTime startTime = DateTime.Now;

            List<Response> result = new List<Response>();

            List<Pair> pairs = DataBase.DataBase.getPairs();

            foreach (Pair pair in pairs)
            {
                Response response = new Response(pair.Id);

                response.Bids = new List<Bid>();

                foreach (int TimeFrame in Config.TimeFrames)
                    response.Bids.AddRange(CheckRate(pair.Id, TimeFrame));

                response.Mounth = new Mounth[12];

                for (int i = 1; i <= 12; i++)
                {
                    response.Mounth[i - 1] = new Mounth(i);
                    response.Mounth[i - 1].Statistics = new List<Statisticscs>();

                    foreach (int TimeFrame in Config.TimeFrames)
                    {
                        List<Bid> bids = response.Bids.FindAll(x => x.Start.Date.Month == i && x.TimeFrame == TimeFrame);

                        response.Mounth[i - 1].Statistics.Add(new Statisticscs(TimeFrame, (float)bids.FindAll(x => x.Success).Count / bids.Count, bids.Count));
                    }
                }

                response.Overall = new Statisticscs(0, (float) response.Bids.FindAll(x => x.Success).Count / response.Bids.Count, response.Bids.Count);

                result.Add(response);
            }

            return result;

            double time = (DateTime.Now - startTime).TotalSeconds;
        }

        static List<Bid> CheckRate(long PairId, int TimeFrame)
        {
            List<Rate> rates = DataBase.DataBase.getRates(1000000, DateTime.Today, PairId, TimeFrame, false);

            return Analysis(rates, TimeFrame);
        }

        static List<Bid> Analysis(List<Rate> Rates, int TimeFrame)
        {
            // Start analys
            
            const int bigSMAPeriod = 45;
            const int smallSMAPeriod = 4;
            const int ratePeriod = 10;
            const float rateMin = (float)0.6;

            List<Bid> result = new List<Bid>();

            for (int i = 0; i < Rates.Count; i++)
            {
                if (i > bigSMAPeriod + 1)
                {
                    float prevBigSMA = CalcSMA(i - bigSMAPeriod - 1, i, bigSMAPeriod, Rates);
                    float prevSmallSMA = CalcSMA(i - smallSMAPeriod - 1, i, smallSMAPeriod, Rates);
                    float curBigSMA = CalcSMA(i - bigSMAPeriod, i, bigSMAPeriod, Rates);
                    float curSmallSMA = CalcSMA(i - smallSMAPeriod, i, smallSMAPeriod, Rates);

                    Boolean prevRatio = (prevBigSMA - prevSmallSMA > 0);
                    Boolean curRatio = (curBigSMA - curSmallSMA > 0);

                    int k = 1;

                    float sum = 0;

                    for (int j = i - ratePeriod; j < i; j++)
                        sum += Math.Abs(Rates[j].Open - Rates[j].Close);

                    float minBody = sum / ratePeriod;

                    // if (i + k < rates.Count && Math.Abs(rates[i].Open - rates[i].Close) * rateMin > minBody)
                    if (i + k < Rates.Count &&
                        //Math.Abs(rates[i].Open - rates[i].Close) * rateMin > minBody &&
                        (Rates[i - 1].Close - Rates[i].Open > 0) == !prevRatio &&
                        // Math.Abs(prevBigSMA - prevSmallSMA) > minBody &&
                        Rates[i].Date.Hour > 9 &&
                        Rates[i].Date.Hour < 20 &&
                        Rates[i].Date.DayOfWeek != DayOfWeek.Saturday &&
                        Rates[i].Date.DayOfWeek != DayOfWeek.Sunday)
                            result.Add(new Bid(TimeFrame, Rates[i], Rates[i + k], (Rates[i].Open - Rates[i + k].Open > 0) == prevRatio));
                }
            }

            /*
            List<Bid> lSuccess = result.FindAll(x => x.Success);
            List<Bid> lNotSuccess = result.FindAll(x => !x.Success);

            float percent = (float) lSuccess.Count / result.Count * 100;
            */

            return result;





            // Finish analys
        }

        static float CalcSMA(int start, int finish, int period, List<Rate> rates)
        {
            float sum = 0;

            for (int i = start; i <= finish; i++)
                sum += rates[i].Close;

            return sum / period;
        }
    }
}