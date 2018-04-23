using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Scalping
{
    class Range
    {
        public float Min;
        public float Max;

        public Range(float Min, float Max)
        {
            this.Min = Min;
            this.Max = Max;
        }
    }

    class Scalping
    {
        private static List<Candlestick> candlesticks = Tests.RisingTrend4PeaksInRow;

        // Минимальнодопустимая длина "волны"
        private const int minWavelength = 5;

        /*
         * Минимальнодопустимое соотношение длин коридоров двух соседних волн.
         * Длина коридора - это растояние от экстремума, максимально отдаленного от прямой,
         * соединяющей точки разворота, до этой прямой. Экстремум считается для конкретной волны.
         */
        private const double lengthCorridorWave = 1.3;

        /*
         * Минимальнодопустимое отклонение средней точки разворота от прямой,
         * соединяющей соседние точки разворота. Может быть как в большую так и в меньшую сторону.
         * Считается как процент, от средней длины разворотных свечей, через которые проводится прямая.
         */
        private const int deviationFromLine = 10;

        /*
         * Анализирует график за прошедший период
         */
        public static List<Bid> HistoryAnalisys()
        {
            List<Bid> bids = new List<Bid>();
            List<Candlestick> reversalPoints = GetReversalPoints();
            List<Combo> combo = new List<Combo>();

            for (int i = 0; i < reversalPoints.Count - 1; i++)
                for (int j = i + 1; j < reversalPoints.Count; j++)
                {
                    /* Заменить, долго будет выполняться */
                    int oneIndex = candlesticks.FindIndex(x => x.Date == reversalPoints[i].Date);
                    int twoIndex = candlesticks.FindIndex(x => x.Date == reversalPoints[j].Date);
                    /* Заменить, долго будет выполняться */

                    if (IsPointsOnLine(oneIndex, twoIndex))
                        combo.Add(
                            new Combo(
                                reversalPoints[i],
                                reversalPoints[j],
                                CalcTurnTime(i, j)
                            )
                        );
                }

            for (int i = 0; i < candlesticks.Count; i++)
            {
                for (int j = 0; j < combo.Count; j++)
                {

                }
            }

            return bids;
        }

        /*
         * Аналирует график в реальном времени
         */
        public static bool RealtimeAnalisys()
        {
            return true;
        }

        class IndexRange
        {
            public int Start;
            public int Finish;

            public IndexRange(int Start, int Finish)
            {
                this.Start = Start;
                this.Finish = Finish;
            }
        }
        /*
         * Определяем точки разворота
         */
        private static List<Candlestick> GetReversalPoints()
        {
            List<Candlestick> result = new List<Candlestick>();
            List<IndexRange> indexes = new List<IndexRange>();

            bool IsExist(int Index)
            {
                bool isExist = false;
                int half = Convert.ToInt32(Math.Floor((float)minWavelength / 2));

                bool IsInRange(int _index, IndexRange Range)
                {
                    return _index >= Range.Start && _index <= Range.Finish;
                }

                for (int i = 0; i < indexes.Count; i++)
                    if (IsInRange(Index, indexes[i]) || IsInRange(Index + half, indexes[i]) || IsInRange(Index - half, indexes[i]))
                            isExist = true;

                return isExist;
            }
            bool CheckMaxInWave(int Index)
            {
                bool maxInWave = true;

                if ((Index - Convert.ToInt32(Math.Floor((float) minWavelength / 2)) < 0) || (Index + Convert.ToInt32(Math.Floor((float)minWavelength / 2)) >= candlesticks.Count))
                    return false;

                for (int i = 1; i <= Convert.ToInt32(Math.Floor((float) minWavelength / 2)); i++)
                    if (candlesticks[Index].High < candlesticks[Index - i].High || candlesticks[Index].High < candlesticks[Index + i].High)
                        maxInWave = false;

                return maxInWave;
            }
            void GetMaxIndex()
            {
                float max = candlesticks.First().High;
                int iMax = 0;

                for (int i = 0; i < candlesticks.Count; i++)
                    if (!IsExist(i) && candlesticks[i].High > max && i < candlesticks.Count - 1 && CheckMaxInWave(i))
                    {
                        max = candlesticks[i].High;
                        iMax = i;
                    }

                if (iMax != 0)
                {
                    result.Add(candlesticks[iMax]);

                    indexes.Add(
                        new IndexRange(
                            iMax - Convert.ToInt32(Math.Floor((float) minWavelength / 2)),
                            iMax + Convert.ToInt32(Math.Floor((float) minWavelength / 2))
                        )
                    );

                    GetMaxIndex();
                }
            }

            /*
             * Определяем максимумы, на которых график разворачивается, растояние между ними должно не меньще мин длины волны (5),
             * фиксируем диапозоны, чтобы при следующем проходе игнорировать уже зафиксированные точки разворота.
             */

            GetMaxIndex();

            return result.OrderBy(x => x.Date).ToList();
        }

        /*
         * ПОКА НЕ РАБОТАЕТ!
         * Ищем экстремумы на периоде
         * @Type (int) StartIndex - индекс, начало периода
         * @Type (int) FinishIndex - индекс, конец периода
         * @Type (bool) Max - Если true, функция вернет максимум для периода
         * @Type (bool) Min - Если true, функция вернет минимум для периода
         */
        private static Candlestick GetExtremumForPeriod(int StartIndex, int FinishIndex, bool Max, bool Min)
        {
            return candlesticks.First();
        }

        /*
         * Проверяем, можно ли провести линию поддержки/сопротивления по указанным точкам разворота
         */
        private static bool IsPointsOnLine(int OneIndex, int TwoIndex)
        {
            /*
             * Разницу между 2-мя максимумаси делим на количество свечей, расположенных между этими максимумами.
             * Получаем значение на которое изменяется график, если провести прямую по максимумам.
             * По этому коэффициенту и определяем точку пересечения определяем.
             */

            float _val, k = (candlesticks[TwoIndex].High - candlesticks[OneIndex].High) / (TwoIndex - OneIndex - 1);

            for (int i = OneIndex + 1; i < TwoIndex; i++)
            {
                if (candlesticks[TwoIndex].High - candlesticks[OneIndex].High > 0)
                    _val = candlesticks[OneIndex].High + (Math.Abs(k) * (i - OneIndex));
                else
                    _val = candlesticks[OneIndex].High - (Math.Abs(k) * (i - OneIndex));

                if (candlesticks[i].High > _val)
                    return false;
            }

            return true;
        }

        /*
         * ПОКА НЕ РАБОТАЕТ!
         * Расчет предполагаемого времени разворота, выражается в количестве свечей
         */
        private static int CalcTurnTime(int OneIndex, int TwoIndex)
        {
            return 1;
        }

        /*
         * Расчитываем допустимый диапозон значений, в котором должно произойти третье касание с линией поддержки/сопротивления
         */
        private static Range CalcAllowableRangeValues(int OneIndex, int TwoIndex)
        {
            return new Range(1, 2);
        }
    }
}