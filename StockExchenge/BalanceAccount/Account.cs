using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.BalanceAccount
{
    public class Balance
    {
        public string Asset { get; set; }
        public double Free { get; set; }
        public double Locked { get; set; }
    }

    public class Account
    {
        public string MakerCommission { get; set; }
        public string TakerCommission { get; set; }
        public string BuyerCommission { get; set; }
        public string SellerCommission { get; set; }
        public bool CanTrade { get; set; }
        public bool CanWithdraw { get; set; }
        public bool CanDeposit { get; set; }

        public string UpdateTime { get; set; }
        public List<Balance> Balances { get; set; }
    }
}
