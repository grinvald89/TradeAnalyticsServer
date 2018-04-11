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
        private static List<Tick> SkipTicks = new List<Tick>();

        private static List<Tick> ticks = new List<Tick>();
        private static List<Tick> ticksOfLastCandlestick = new List<Tick>();
        private static List<Candlestick> candlesticks = new List<Candlestick>();

        const int pairId = 12;
        const int MaxBigSMAPeriod = 60;
        const float KShadowToBody = (float) 0.25;

        public static void StartHistoryAnalysis(DateTime StartDate, DateTime FinishDate)
        {
            List<Result> results = new List<Result>();

            // Перебираем Тики по дням
            for (int day = 0; day <= (FinishDate - StartDate).Days; day++)
            {
                ticks = DataBase.DataBase.GetTicks("OlympTradeTicks", 10000000, StartDate.AddDays(day), pairId);

                if (ticks.Count > 0)
                {
                    // Удаляем одинаковые тики, которые идут ПОСЛЕДОВАТЕЛЬНО, посекундно
                    RemoveSameTicks();

                    // Перебираем комбинации по большой скользящей средней
                    for (int bigSMA = 12; bigSMA <= MaxBigSMAPeriod; bigSMA++)
                        // Перебираем комбинации по малой скользящей средней
                        for (int smallSMA = 2; smallSMA <= 10; smallSMA++)
                            // Перебираем комбинации по соотношению длины тени к телу предыдущей свечи
                            for (int j = 0; j < 8; j++)
                                // Перебираем комбинации по продолжительности ставки
                                for (int k = 1; k <= 3; k++)
                                {
                                    List<Bid> res = HistoryAnalysis(bigSMA, smallSMA, 5, k, j);

                                    Day resultDay = new Day((float) res.FindAll(x => x.Success).Count / res.Count * 100, res.Count, res);

                                    int resultIndex = results.FindIndex(x => x.BigSMAPeriod == bigSMA && x.SmallSMAPeriod == smallSMA && x.K == k && x.ShadowToBody == j);

                                    if (resultIndex != -1)
                                        results[resultIndex].Days.Add(resultDay);
                                    else
                                        results.Add(new Result(bigSMA, smallSMA, k, resultDay, j));
                                }
                }
            }

            // Расчет общего процента и количества сделок по всем дням
            foreach (Result result in results)
            {
                List<Bid> bids = new List<Bid>();

                foreach (Day day in result.Days)
                    bids.AddRange(day.Bids);

                result.Percent = (float) bids.FindAll(x => x.Success).Count / bids.Count;
                result.Count = bids.Count;
            }

            List<Result> resultsOverall = results.OrderByDescending(x => x.Percent).ToList();

            #region Проверяем воспроизведение наилучших стратегий
            List<NextResult> repeatabilityAnalysis2Day10 = CalcReproducibilityAnalysis(resultsOverall, 2, 10);
            List<NextResult> repeatabilityAnalysis2Day100 = CalcReproducibilityAnalysis(resultsOverall, 2, 100);

            List<NextResult> repeatabilityAnalysis3Day10 = CalcReproducibilityAnalysis(resultsOverall, 3, 10);
            List<NextResult> repeatabilityAnalysis3Day100 = CalcReproducibilityAnalysis(resultsOverall, 3, 100);

            List<NextResult> repeatabilityAnalysis4Day10 = CalcReproducibilityAnalysis(resultsOverall, 4, 10);
            List<NextResult> repeatabilityAnalysis4Day100 = CalcReproducibilityAnalysis(resultsOverall, 4, 100);

            List<NextResult> repeatabilityAnalysis5Day10 = CalcReproducibilityAnalysis(resultsOverall, 5, 10);
            List<NextResult> repeatabilityAnalysis5Day100 = CalcReproducibilityAnalysis(resultsOverall, 5, 100);

            List<NextResult> repeatabilityAnalysis6Day10 = CalcReproducibilityAnalysis(resultsOverall, 6, 10);
            List<NextResult> repeatabilityAnalysis6Day100 = CalcReproducibilityAnalysis(resultsOverall, 6, 100);

            List<NextResult> repeatabilityAnalysis7Day10 = CalcReproducibilityAnalysis(resultsOverall, 7, 10);
            List<NextResult> repeatabilityAnalysis7Day100 = CalcReproducibilityAnalysis(resultsOverall, 7, 100);
            #endregion
        }

        private static List<Bid> HistoryAnalysis(int BigSMAPeriod, int SmallSMAPeriod, int TimeFrame, int K, int ShadowToBody)
        {
            List<Bid> result = new List<Bid>();

            for (int i = 0; i < ticks.Count; i++)
            {
                if (ticks[i].Date.Hour > 5)
                {
                    if (candlesticks.Count > BigSMAPeriod)
                    {
                        float ShadowToBodyMin = (float) (1 + ShadowToBody * KShadowToBody);
                        float ShadowToBodyMax = (float) (1 + KShadowToBody + ShadowToBody * KShadowToBody);

                        if (Candlestick.GetShadowToBody(candlesticks.Last()) > ShadowToBodyMin && Candlestick.GetShadowToBody(candlesticks.Last()) < ShadowToBodyMax)
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

                            if ((prevBigSMA - prevSmallSMA > 0) != ((currBigSMA - currSmallSMA) > 0))
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
                                        ShadowToBodyMin,
                                        ShadowToBodyMax
                                    ));

                                /* Подсчет пропущенных элементов */
                                if (resultTick == null && ticks[i].Date.Hour < 23)
                                    SkipTicks.Add(ticks[i]);
                                /* Подсчет пропущенных элементов */
                            }
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
            int sec = ticks[CurrentIndex].Date.Second + 2;
            nextDate = nextDate.AddSeconds(sec);
            List<Tick> nextTicks = new List<Tick>();

            for (int i = CurrentIndex + 1; i < ticks.Count - 1 && ticks[i].Date.CompareTo(nextDate) < 0; i++)
                if (ticks[i].Date.Day == nextDate.Day &&
                    ticks[i].Date.Hour == nextDate.Hour &&
                    ticks[i].Date.Minute == nextDate.Minute)
                    nextTicks.Add(ticks[i]);

            // Доп условие
            if (nextTicks.Count == 0) {
                for (int i = CurrentIndex + 1; i < ticks.Count - 1 && ticks[i].Date.CompareTo(nextDate.AddSeconds(10)) < 0; i++)
                    if (ticks[i].Date.CompareTo(nextDate) > 0)
                        nextTicks.Add(ticks[i]);

                return (nextTicks.Count > 0) ? nextTicks.First() : null;
            }
            else
                return nextTicks.Last();
        }

        private static float CalcStatForPeriod(List<Day> Days, int StartIndexDay, int FinishIndexDay)
        {
            List<Bid> bids = new List<Bid>();

            for (int i = StartIndexDay; i < FinishIndexDay && i < Days.Count; i++)
                bids.AddRange(Days[i].Bids);

            return (float)bids.FindAll(x => x.Success).Count / bids.Count;
        }

        private static NextResult CalcStatForNextDay(List<Result> List, int Day, int BidsCount)
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

                res.FullPercent = (float)fullBids.FindAll(x => x.Success).Count / fullBids.Count;
                res.FullCount = fullBids.Count;

                res.Percent = (float)bids.FindAll(x => x.Success).Count / bids.Count;
                res.Count = bids.Count;

                res.FullBids = fullBids;
                res.FirstBids = bids;

                res.ResultList = List;

                return res;
            }
            else
                return null;
        }

        private static List<NextResult> CalcReproducibilityAnalysis(List<Result> List, int Period, int BidsCount)
        {
            List<NextResult> resultList = new List<NextResult>();

            int DayCount = List.First().Days.Count;

            for (int i = 0; i < DayCount - Period; i++)
                resultList.Add(CalcStatForNextDay(
                    List.OrderByDescending(x => CalcStatForPeriod(x.Days, i, i + Period)).ToList(),
                    i + Period,
                    BidsCount
                ));

            return resultList;
        }
    }
}