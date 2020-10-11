using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.BackTest
{
    public class Statistics
    {
        readonly List<Position> positions;
        readonly double initiallyBalance;
        public Statistics(List<Position> positions, double initiallyBalance)
        {
            this.positions = positions;
            this.initiallyBalance = initiallyBalance;
        }

        public void CalcStatistics()
        {
            SetCountProfit();
            SetCountLoss();
            SetExtremumProfit();
            SetTotalGross();
            SetStdDevProfit();
            SetAvgProfitTrade();
            SetAvgLossTrade();
        }

        private void SetCountProfit()
        {
            CountProfit = positions.Where(x => x.Profit > 0).Count();
        }
        private void SetCountLoss()
        {
            CountLoss = positions.Where(x => x.Profit < 0).Count();
        }

        double maxProfit = 0;
        double maxLoss = 0;
        double profit = 0;
        private void SetExtremumProfit()
        {
            var positionSorts = positions.Where(x=>x.IsClose).OrderBy(x => x.ID);
            foreach (var position in positionSorts)
            {
                profit += position.Profit;
                if(profit > maxProfit)
                {
                    maxProfit = profit;
                }
                if(profit < maxLoss)
                {
                    maxLoss = profit;
                }
            }
            MaxProfit = (maxProfit * 100) / initiallyBalance;
            MaxLoss = (maxLoss * 100) / initiallyBalance;
        }
        private void SetTotalGross()
        {
            TotalGross = positions.Sum(x => x.Profit);
        }
        private void SetStdDevProfit()
        {
            if(positions != null)
            {
                if (positions.Count > 0)
                {
                    var profitPositions = new List<double>();
                    foreach (var position in positions)
                    {
                        var profitProc = (position.Profit * 100) / initiallyBalance;
                        profitPositions.Add(profitProc);
                    }
                    var stddev = Math.Sqrt(profitPositions.Average(z => z * z) - Math.Pow(profitPositions.Average(), 2));
                    StdDevProfit = stddev;
                }
            }
        }
        private void SetAvgProfitTrade()
        {
            AvgProfitTrade = positions.Where(x => x.Profit > 0).Average(x => x.Profit);
        }
        private void SetAvgLossTrade()
        {
            AvgLossTrade = positions.Where(x => x.Profit < 0).Average(x => x.Profit);
        }

        /// <summary>
        /// Количество прибыльных сделок
        /// </summary>
        public int CountProfit { get; private set; }
        /// <summary>
        /// Количество убыточных сделок
        /// </summary>
        public int CountLoss { get; private set; }
        /// <summary>
        /// Максимальная прибыль по счету
        /// </summary>
        public double MaxProfit { get; private set; }
        /// <summary>
        /// Максимальный убыток по счету (просадка)
        /// </summary>
        public double MaxLoss { get; private set; }
        /// <summary>
        /// Итог без комиссии
        /// </summary>
        public double TotalGross { get; private set; }
        /// <summary>
        /// Средняя прибыль на сделку
        /// </summary>
        public double AvgProfitTrade { get; private set; }
        /// <summary>
        /// Средний убыток на сделку
        /// </summary>
        public double AvgLossTrade { get; private set; }
        /// <summary>
        /// Стандартное отклонение прибыли/убытка по сделкам
        /// </summary>
        public double StdDevProfit { get; private set; }
    }
}
