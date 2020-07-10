using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge
{
    public class ExchangeInfo
    {
        public string Info { get; private set; }
        private readonly PublicRequester publicRequester;
        public ExchangeInfo()
        {
            publicRequester = new PublicRequester();
        }
        public string GetInfo()
        {
            try
            {
                var response = publicRequester.RequestPublicApi($"https://api.binance.com/api/v1/exchangeInfo");
                Info = response;
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
