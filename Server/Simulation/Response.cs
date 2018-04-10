using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Simulation
{
    class Statistics
    {
        public int TimeFrame;
        public float Percent;
        public int Count;

        public Statistics(int TimeFrame, float Percent, int Count)
        {
            this.TimeFrame = TimeFrame;
            this.Percent = Percent;
            this.Count = Count;
        }
    }

    /*
     * @Type TimeFrame - Таймфрейм, на котором происходит анализ
     * @Type Start - Тик, на котором делаем ставку
     * @Type Finish - Тик, на котором считываем результат
     * @Type Success - Результат сделки
     * @Type Success - Результат сделки
     * @Type BigSMA - Период большой скользящей средней
     * @Type SmallSMA - Период малой скользящей средней
     * @Type K - Коэффициент, на который умножаем таймфрейм для текущей сделки,
     *      т.е. при таймфрейме 5 минут и K=1, делаем ставку на 5 минут; при K=2 - делаем ставку на 10 минут
     * @Type ShadowToBodyMin - Минимально допустимое соотношение длины тени к длине тела предыдущей свечу ( = shadow / body )
     * @Type ShadowToBodyMax - Максимально допустимое соотношение длины тени к длине тела предыдущей свечу ( = shadow / body )
     */
    class Bid
    {
        public int TimeFrame;
        public Tick Start;
        public Tick Finish;
        public bool Success;
        public int BigSMA;
        public int SmallSMA;
        public int K;
        public float ShadowToBodyMin;
        public float ShadowToBodyMax;

        public Bid(int TimeFrame, Tick Start, Tick Finish, bool Success, int BigSMA, int SmallSMA, int K, float ShadowToBodyMin, float ShadowToBodyMax)
        {
            this.TimeFrame = TimeFrame;
            this.Start = Start;
            this.Finish = Finish;
            this.Success = Success;
            this.BigSMA = BigSMA;
            this.SmallSMA = SmallSMA;
            this.K = K;
            this.ShadowToBodyMin = ShadowToBodyMin;
            this.ShadowToBodyMax = ShadowToBodyMax;
        }
    }

    class Mounth
    {
        public int Number;
        public List<Statistics> Statistics;

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
        public Statistics[] Overall;

        public Response(long PairId)
        {
            this.PairId = PairId;
            this.Bids = new List<Bid>();
            this.Mounth = new Mounth[12];
            this.Overall = new Statistics[7];
        }
    }
}