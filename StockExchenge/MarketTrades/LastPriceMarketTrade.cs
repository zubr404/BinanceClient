using Services;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketTrades
{
    /// <summary>
    /// Цена последней сделки. GET
    /// </summary>
    public class LastPriceMarketTrade
    {
        private readonly PublicRequester publicRequester;
        public LastPriceMarketTrade()
        {
            publicRequester = new PublicRequester();
        }
        public LastPriceResponse GetInfo(string pair)
        {
            try
            {
                var response = publicRequester.RequestPublicApi($"https://api.binance.com/api/v3/ticker/price?symbol={pair}");
                return JConverter.JsonConver<LastPriceResponse>(response);
            }
            catch (Exception ex)
            {
                // TODO: Сохранеие логово
            }
            return null;
        }
    }

    // {"symbol":"BNBBTC","price":"0.00207970"}
    public class LastPriceResponse
    {
        public string symbol { get; set; }
        public double price { get; set; }
    }
}
