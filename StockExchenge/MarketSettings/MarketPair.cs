using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketSettings
{
    /*
    ....
    ....
    "symbol": "ETHBTC",
    "status": "TRADING",
    "baseAsset": "ETH",
    "baseAssetPrecision": 8,
    "quoteAsset": "BTC",
    "quotePrecision": 8,
    "orderTypes": ["LIMIT", "MARKET"],
    "icebergAllowed": false,
    ....
    ....
    */
    public class MarketPair
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string Pair { get; set; }
        public string Status { get; set; } // "status": "TRADING"
    }
}
