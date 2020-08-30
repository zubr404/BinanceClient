using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class Trade
    {
        public int ID { get; set; }
        public string FK_PublicKey { get; set; }
        public string Symbol { get; set; }
        public string TradeID { get; set; }
        public string OrderID { get; set; }
        public string OrderListID { get; set; }
        public string Price { get; set; }
        public string Qty { get; set; }
        public string QuoteQty { get; set; }
        public string Commission { get; set; }
        public string CommissionAsset { get; set; }
        public string Time { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsMaker { get; set; }
        public bool IsBestMatch { get; set; }
        public APIKey APIKey { get; set; }
    }
}
