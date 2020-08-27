using StockExchenge;
using StockExchenge.BalanceAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.Services
{
    public class BalanceService
    {
        readonly AccountInfo accountInfo;

        public BalanceService(AccountInfo accountInfo)
        {
            this.accountInfo = accountInfo;
        }

        public void GetBalance()
        {
            accountInfo.RequestedBalances();
        }
    }
}
