using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel.ScrinCalculator
{
    public class BackTestResultView : PropertyChangedBase
    {
        #region Текущая сетка
        private List<OrderDataView> currentGridDatas;
        /// <summary>
        /// Отображение возможной текущей сетки
        /// </summary>
        public List<OrderDataView> CurrentGridDatas
        {
            get { return currentGridDatas; }
            set
            {
                if (value != null)
                {
                    currentGridDatas = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private double depositUsed;
        public double DepositUsed
        {
            get { return Math.Round(depositUsed, 10); }
            set
            {
                depositUsed = value;
                base.NotifyPropertyChanged();
            }
        }

        private int numberOfOrders;
        public int NumberOfOrders
        {
            get { return numberOfOrders; }
            set
            {
                numberOfOrders = value;
                base.NotifyPropertyChanged();
            }
        }

        private double coverageOfPriceReduction;
        public double CoverageOfPriceReduction
        {
            get { return Math.Round(coverageOfPriceReduction, 2); }
            set
            {
                coverageOfPriceReduction = value;
                base.NotifyPropertyChanged();
            }
        }
        #endregion

        #region Бектест
        private int countProfit;
        /// <summary>
        /// Количество прибыльных сделок
        /// </summary>
        public int CountProfit
        {
            get { return countProfit; }
            set
            {
                countProfit = value;
                base.NotifyPropertyChanged();
            }
        }

        private int countLoss;
        /// <summary>
        /// Количество убыточных сделок
        /// </summary>
        public int CountLoss
        {
            get { return countLoss; }
            set
            {
                countLoss = value;
                base.NotifyPropertyChanged();
            }
        }

        private double maxProfit;
        /// <summary>
        /// Максимальная прибыль по счету
        /// </summary>
        public double MaxProfit
        {
            get { return Math.Round(maxProfit, 2); }
            set
            {
                maxProfit = value;
                base.NotifyPropertyChanged();
            }
        }

        private double maxLoss;
        /// <summary>
        /// Максимальный убыток по счету (просадка)
        /// </summary>
        public double MaxLoss
        {
            get { return Math.Round(maxLoss, 2); }
            set
            {
                maxLoss = value;
                base.NotifyPropertyChanged();
            }
        }

        private double totalGross;
        /// <summary>
        /// Итог без комиссии
        /// </summary>
        public double TotalGross
        {
            get { return Math.Round(totalGross, 2); }
            set
            {
                totalGross = value;
                base.NotifyPropertyChanged();
            }
        }

        private double avgProfitTrade;
        /// <summary>
        /// Средняя прибыль на сделку
        /// </summary>
        public double AvgProfitTrade
        {
            get { return Math.Round(avgProfitTrade, 2); }
            set
            {
                avgProfitTrade = value;
                base.NotifyPropertyChanged();
            }
        }

        private double avgLossTrade;
        /// <summary>
        /// Средний убыток на сделку
        /// </summary>
        public double AvgLossTrade
        {
            get { return Math.Round(avgLossTrade, 2); }
            set
            {
                avgLossTrade = value;
                base.NotifyPropertyChanged();
            }
        }

        private double stdDevProfit;
        /// <summary>
        /// Стандартное отклонение прибыли/убытка по сделкам
        /// </summary>
        public double StdDevProfit
        {
            get { return Math.Round(stdDevProfit, 2); }
            set
            {
                stdDevProfit = value;
                base.NotifyPropertyChanged();
            }
        }
        #endregion
    }
}
