using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class NextResult
    {
        public float Percent;
        public int Count;
        public float FullPercent;
        public int FullCount;
        public List<Bid> FullBids;
        public List<Bid> FirstBids;
        public List<Result> ResultList;
    }

    class Day
    {
        public float Percent;
        public int Count;
        public List<Bid> Bids;

        public Day(float Percent, int Count, List<Bid> Bids)
        {
            this.Percent = Percent;
            this.Count = Count;
            this.Bids = Bids;
        }
    }

    class Result
    {
        public int BigSMAPeriod;
        public int SmallSMAPeriod;
        public float Percent;
        public int Count;
        public int K;
        public List<Day> Days;
        public float ShadowToBody;

        public Result(int BigSMAPeriod, int SmallSMAPeriod, int K, Day Day, float ShadowToBody)
        {
            this.BigSMAPeriod = BigSMAPeriod;
            this.SmallSMAPeriod = SmallSMAPeriod;
            this.K = K;
            Days = new List<Day>();
            Days.Add(Day);
            this.ShadowToBody = ShadowToBody;
        }
    }

    class Simulation
    {
        private static List<Tick> ticks = new List<Tick>();
        private static List<Tick> ticksOfLastCandlestick = new List<Tick>();
        private static List<Candlestick> candlesticks = new List<Candlestick>();

        const int MaxBigSMAPeriod = 60;

        const int pairId = 12;

        public static void Start()
        {
            DateTime startDate = new DateTime(2018, 03, 23);
            DateTime finishDate = new DateTime(2018, 04, 09);

            int dayCount = (finishDate - startDate).Days;

            List<Result> results = new List<Result>();

            for (int day = 0; day <= (finishDate - startDate).Days; day++)
            {
                try
                {
                    ticks = DataBase.DataBase.GetTicks("OlympTradeTicks", 10000000, startDate.AddDays(day), pairId);
                }
                catch
                {
                    ticks.Clear();
                }                

                if (ticks.Count > 0)
                {
                    RemoveSameTicks();

                    for (int bigSMA = 12; bigSMA <= MaxBigSMAPeriod; bigSMA++)
                        for (int smallSMA = 2; smallSMA <= 10; smallSMA++)
                            for (int j = 0; j < 8; j++)
                                for (int k = 1; k <= 3; k++)
                                {
                                    List<Bid> res = Analysis(bigSMA, smallSMA, 5, k, j);

                                    Day resultDay = new Day((float)res.FindAll(x => x.Success).Count / res.Count * 100, res.Count, res);

                                    int resultIndex = results.FindIndex(x => x.BigSMAPeriod == bigSMA && x.SmallSMAPeriod == smallSMA && x.K == k && x.ShadowToBody == j);

                                    if (resultIndex != -1)
                                        results[resultIndex].Days.Add(resultDay);
                                    else
                                        results.Add(new Result(bigSMA, smallSMA, k, resultDay, j));
                                }
                }
            }


            foreach (Result result in results)
            {
                List<Bid> bids = new List<Bid>();

                foreach (Day _day in result.Days)
                    bids.AddRange(_day.Bids);

                result.Percent = (float) bids.FindAll(x => x.Success).Count / bids.Count;
                result.Count = bids.Count;
            }


            float CalcPercentForDays(List<Day> Days, int StartIndexDay, int FinishIndexDay)
            {
                List<Bid> bids = new List<Bid>();

                for (int i = StartIndexDay; i < FinishIndexDay && i < Days.Count; i++)
                    bids.AddRange(Days[i].Bids);

                return (float) bids.FindAll(x => x.Success).Count / bids.Count;
            }

            NextResult CalcPercentForNextDays(List<Result> List, int Day, int BidsCount)
            {
                if (Day < List.First().Days.Count)
                {
                    List<Bid> fullBids = new List<Bid>();
                    List<Bid> bids = new List<Bid>();

                    for (int i = 0; i < BidsCount; i++)
                    {
                        fullBids.AddRange(List[i].Days[Day].Bids);

                        if (List[i].Days[Day].Bids.Count > 0)
                            bids.Add(List[i].Days[Day].Bids.First());
                    }

                    NextResult res = new NextResult();

                    res.FullPercent = (float) fullBids.FindAll(x => x.Success).Count / fullBids.Count;
                    res.FullCount = fullBids.Count;

                    res.Percent = (float) bids.FindAll(x => x.Success).Count / bids.Count;
                    res.Count = bids.Count;

                    res.FullBids = fullBids;
                    res.FirstBids = bids;

                    res.ResultList = List;

                    return res;
                }
                else
                    return null;
            }


            List<Result> resultsOverall = results.OrderByDescending(x => x.Percent).ToList();

            List<Result> results3day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 3)).ToList();
            NextResult day3analys10 = CalcPercentForNextDays(results3day, 3, 10);
            NextResult day3analys100 = CalcPercentForNextDays(results3day, 3, 100);


            List<Result> results4day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 4)).ToList();
            NextResult day4analys10 = CalcPercentForNextDays(results4day, 4, 10);
            NextResult day4analys100 = CalcPercentForNextDays(results4day, 4, 100);

            
            List<Result> results5day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 5)).ToList();
            NextResult day5analys10 = CalcPercentForNextDays(results5day, 5, 10);
            NextResult day5analys100 = CalcPercentForNextDays(results5day, 5, 100);


            List<Result> results6day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 6)).ToList();
            NextResult day6analys10 = CalcPercentForNextDays(results6day, 6, 10);
            NextResult day6analys100 = CalcPercentForNextDays(results6day, 6, 100);


            List<Result> results7day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 7)).ToList();
            NextResult day7analys10 = CalcPercentForNextDays(results7day, 7, 10);
            NextResult day7analys100 = CalcPercentForNextDays(results7day, 7, 100);


            List<Result> results8day = results.OrderByDescending(x => CalcPercentForDays(x.Days, 0, 8)).ToList();
            NextResult day8analys10 = CalcPercentForNextDays(results8day, 8, 10);
            NextResult day8analys100 = CalcPercentForNextDays(results8day, 8, 100);


            List<NextResult> CalcRepeatabilityAnalysis(List<Result> List, int Period, int BidsCount)
            {
                List<NextResult> resultList = new List<NextResult>();

                int DayCount = List.First().Days.Count;

                for (int i = 0; i < DayCount - Period; i++)
                    resultList.Add(CalcPercentForNextDays(
                        List.OrderByDescending(x => CalcPercentForDays(x.Days, i, i + Period)).ToList(),
                        i + Period,
                        BidsCount
                    ));

                return resultList;
            }

            int dayCountInList = resultsOverall.First().Days.Count;

            List<NextResult> repeatabilityAnalysis2Day10 = CalcRepeatabilityAnalysis(resultsOverall, 2, 10);
            List<NextResult> repeatabilityAnalysis2Day100 = CalcRepeatabilityAnalysis(resultsOverall, 2, 100);


            List<NextResult> repeatabilityAnalysis3Day10 = CalcRepeatabilityAnalysis(resultsOverall, 3, 10);
            List<NextResult> repeatabilityAnalysis3Day100 = CalcRepeatabilityAnalysis(resultsOverall, 3, 100);


            List<NextResult> repeatabilityAnalysis4Day10 = CalcRepeatabilityAnalysis(resultsOverall, 4, 10);
            List<NextResult> repeatabilityAnalysis4Day100 = CalcRepeatabilityAnalysis(resultsOverall, 4, 100);

            List<Result> l4_1 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 4, dayCountInList)).ToList();
            List<Result> l4_2 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 5, dayCountInList - 1)).ToList();


            List<NextResult> repeatabilityAnalysis5Day10 = CalcRepeatabilityAnalysis(resultsOverall, 5, 10);
            List<NextResult> repeatabilityAnalysis5Day100 = CalcRepeatabilityAnalysis(resultsOverall, 5, 100);

            List<Result> l5_1 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 5, dayCountInList)).ToList();
            List<Result> l5_2 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 6, dayCountInList - 1)).ToList();


            List<NextResult> repeatabilityAnalysis6Day10 = CalcRepeatabilityAnalysis(resultsOverall, 6, 10);
            List<NextResult> repeatabilityAnalysis6Day100 = CalcRepeatabilityAnalysis(resultsOverall, 6, 100);

            List<Result> l6_1 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 6, dayCountInList)).ToList();
            List<Result> l6_2 = resultsOverall.OrderByDescending(x => CalcPercentForDays(x.Days, dayCountInList - 7, dayCountInList - 1)).ToList();


            List<NextResult> repeatabilityAnalysis7Day10 = CalcRepeatabilityAnalysis(resultsOverall, 7, 10);
            List<NextResult> repeatabilityAnalysis7Day100 = CalcRepeatabilityAnalysis(resultsOverall, 7, 100);
        }

        /*
         * K - сколько свечей пропускаем, 1 - не пропускаем
         */
        private static List<Bid> Analysis(int BigSMAPeriod, int SmallSMAPeriod, int TimeFrame, int K, int ShadowToBody)
        {
            List<Bid> result = new List<Bid>();

            for (int i = 0; i < ticks.Count; i++)
            {
                if (candlesticks.Count > BigSMAPeriod)
                {
                    float currBigSMA = Indicators.SMA.CalcTicks(
                            candlesticks.GetRange(candlesticks.Count - BigSMAPeriod, BigSMAPeriod),
                            ticks[i]
                        );

                    float currSmallSMA = Indicators.SMA.CalcTicks(
                            candlesticks.GetRange(candlesticks.Count - SmallSMAPeriod, SmallSMAPeriod),
                            ticks[i]
                        );

                    float prevBigSMA = Indicators.SMA.CalcTicks(candlesticks.GetRange(candlesticks.Count - BigSMAPeriod, BigSMAPeriod));

                    float prevSmallSMA = Indicators.SMA.CalcTicks(candlesticks.GetRange(candlesticks.Count - SmallSMAPeriod, SmallSMAPeriod));

                    if ((prevBigSMA - prevSmallSMA > 0) != ((currBigSMA - currSmallSMA) > 0) && ticks[i].Date.Hour > 5)
                    {
                        float _k = (float) (1 + ShadowToBody * 0.25);
                        float _shadowToBody = Candlestick.GetShadowToBody(candlesticks.Last());

                        if (_shadowToBody > _k && _shadowToBody < (float) (_k + 0.25))
                        {
                            Tick resultTick = GetNextTick(i, TimeFrame, K);

                            if (resultTick != null && (result.Count == 0 || (ticks[i].Date - result.Last().Finish.Date).Minutes >= TimeFrame))
                                result.Add(new Bid(
                                    TimeFrame,
                                    ticks[i],
                                    resultTick,
                                    (ticks[i].Value - resultTick.Value > 0) != (currBigSMA - currSmallSMA > 0),
                                    BigSMAPeriod,
                                    SmallSMAPeriod,
                                    K,
                                    (float) (1 + ShadowToBody * 0.25),
                                    (float) (1.25 + ShadowToBody * 0.25)
                                ));
                        }
                    }
                }

                AddTickToCandlesticks(ticks[i], TimeFrame);
            }

            return result;
        }


        private static void AddTickToCandlesticks(Tick Tick, int TimeFrame)
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

                if (DateEqual(candlesticks.Last().Date, Tick.Date) && Tick.Date.Minute % TimeFrame == 0)
                    CalcLastCandlestick(TimeFrame);

                ticksOfLastCandlestick.Add(Tick);
            }
            else
            {
                if (ticksOfLastCandlestick.Count > 0 && candlesticks.Count == 0 && ticksOfLastCandlestick.Last().Date.Second > Tick.Date.Second)
                    CalcLastCandlestick(TimeFrame);

                ticksOfLastCandlestick.Add(Tick);
            }
        }

        private static void CalcLastCandlestick(int TimeFrame)
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
                TimeFrame)
            );

            if (candlesticks.Count > MaxBigSMAPeriod)
                candlesticks.RemoveAt(0);

            ticksOfLastCandlestick.Clear();
        }

        private static void RemoveSameTicks()
        {
            Tick tick = ticks.First();

            List<int> removeIndexes = new List<int>();

            for (int i = 1; i < ticks.Count; i++)
            {
                if (tick.Date.Year == ticks[i].Date.Year &&
                    tick.Date.Month == ticks[i].Date.Month &&
                    tick.Date.Day == ticks[i].Date.Day &&
                    tick.Date.Hour == ticks[i].Date.Hour &&
                    tick.Date.Minute == ticks[i].Date.Minute &&
                    tick.Date.Second == ticks[i].Date.Second)
                {
                    if (tick.Value == ticks[i].Value)
                        removeIndexes.Add(i);
                    else
                        tick = ticks[i];
                }
                else
                    tick = ticks[i];
            }

            for (int i = removeIndexes.Count - 1; i >= 0; i--)
                ticks.RemoveAt(removeIndexes[i]);
        }

        private static Tick GetNextTick(int CurrentIndex, int TimeFrame, int K)
        {
            DateTime nextDate = ticks[CurrentIndex].Date.AddMinutes(TimeFrame * K);
            nextDate.AddSeconds(ticks[CurrentIndex].Date.Second + 2);
            List<Tick> nextTicks = new List<Tick>();

            for (int i = CurrentIndex + 1; i < ticks.Count - 1 && ticks[i].Date.CompareTo(nextDate) < 0; i++)
                if (ticks[i].Date.Day == nextDate.Day &&
                    ticks[i].Date.Hour == nextDate.Hour &&
                    ticks[i].Date.Minute == nextDate.Minute)
                        nextTicks.Add(ticks[i]);

            return  (nextTicks.Count > 0) ? nextTicks.Last() : null;
        }
    }
}