using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.TradeAccount
{
    /*
     {
        "symbol": "BNBBTC",
        "id": 28457,
        "orderId": 100234,
        "orderListId": -1,
        "price": "4.00000100",
        "qty": "12.00000000",
        "quoteQty": "48.000012",
        "commission": "10.10000000",
        "commissionAsset": "BNB",
        "time": 1499865549590,
        "isBuyer": true,
        "isMaker": false,
        "isBestMatch": true
    }
     */
    public class Trade
    {
        public string symbol { get; set; }
        public long id { get; set; }
        public long orderId { get; set; }
        public int orderListId { get; set; }
        public double price { get; set; }
        public double qty { get; set; }
        public double quoteQty { get; set; }
        public double commission { get; set; }
        public string commissionAsset { get; set; }
        public long time { get; set; }
        public bool isBuyer { get; set; }
        public bool isMaker { get; set; }
        public bool isBestMatch { get; set; }
    }
}
