using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class TradeConfiguration
    {
        public int ID { get; set; }
        public string MainCoin { get; set; }
        public string AltCoin { get; set; }
        public string Strategy { get; set; }
        public int IntervalHttp { get; set; }
        public bool Margin { get; set; }
        public int OpenOrders { get; set; }
        public double OrderIndent { get; set; }
        public double OrderDeposit { get; set; }
        public double FirstStep { get; set; }
        public double OrderStepPlus { get; set; }
        public double Martingale { get; set; }
        public double DepositLimit { get; set; }
        public double OrderReload { get; set; }
        public double Loss { get; set; }
        public double Profit { get; set; }
        public double IndentExtremum { get; set; }
        public double ProtectiveSpread { get; set; }
        public bool Active { get; set; }
        public long ActivationTime { get; set; }
        public long DeactivationTime { get; set; }
    }
}
