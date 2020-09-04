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
        public long TradeID { get; set; }
        public long OrderID { get; set; }
        public int OrderListID { get; set; }
        public double Price { get; set; }
        public double Qty { get; set; }
        public double QuoteQty { get; set; }
        public double Commission { get; set; }
        public string CommissionAsset { get; set; }
        public long Time { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsMaker { get; set; }
        public bool IsBestMatch { get; set; }
        public APIKey APIKey { get; set; }
    }
}
