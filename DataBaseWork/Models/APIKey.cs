using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class APIKey
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public List<Trade> Trades { get; set; }
        public List<Balance> Balances { get; set; }
        public List<StopLimitOrder> StopLimitOrders { get; set; }
        public List<TakeProfitOrder> TakeProfitOrders { get; set; }

        public APIKey()
        {
            Trades = new List<Trade>();
            Balances = new List<Balance>();
            StopLimitOrders = new List<StopLimitOrder>();
            TakeProfitOrders = new List<TakeProfitOrder>();
        }
    }
}
