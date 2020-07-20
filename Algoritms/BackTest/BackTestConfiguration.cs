using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms.BackTest
{
    public class BackTestConfiguration
    {
        public GeneralSetting GeneralSetting { get; set; }
        public string MainCoin { get; set; }
        public string AltCoin { get; set; }
        public int StrategyID { get; set; }
        public int OpenOrders { get; set; }
        public double OrderIndent { get; set; }
        public double OrderDeposit { get; set; }
        public double OrderStep { get; set; }
        public double OrderStepPlus { get; set; }
        public double Martingale { get; set; }
        public double DepositLimit { get; set; }
        public double OrderReload { get; set; }
        public double StopLoss { get; set; }
        public double Profit { get; set; }
        public double Tralling { get; set; }
        public bool Active { get; set; }
    }

    public class GeneralSetting
    {
        /// <summary>
        /// Общий размер счета
        /// </summary>
        public double Deposit { get; set; }
        /// <summary>
        /// Начальная точка исторических данных (timestamp)
        /// </summary>
        public long StartTime { get; set; }
        /// <summary>
        /// Конечная точка исторических данных (timestamp)
        /// </summary>
        public long StopTime { get; set; }
    }
}
