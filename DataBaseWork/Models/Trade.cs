using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class Trade
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Symbol { get; set; }
        public int TradeID { get; set; }
        public int OrderID { get; set; }
        public int OrderListID { get; set; }
        public string Price { get; set; }
        public string Qty { get; set; }
        public string QuoteQty { get; set; }
        public string Commission { get; set; }
        public string CommissionAsset { get; set; }
        public int Time { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsMarket { get; set; }
        public bool IsBestMatch { get; set; }
    }
}
