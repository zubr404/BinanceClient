using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class TradeConfiguration
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string MainCoin { get; set; }
        public string AltCoin { get; set; }
        public int StrategyID { get; set; }
        public int IntervalHttp { get; set; }
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
        public Strategy Strategy { get; set; }
    }
}
