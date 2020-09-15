using Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketSettings
{
    public class AllPairsMarket
    {

        public const string STATUS_TRADING = "TRADING";
        public List<MarketPair> MarketPairs { get; private set; }

        public AllPairsMarket(string data)
        {
            MarketPairs = new List<MarketPair>();
            GetPairs(data);
        }

        private void GetPairs(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                try
                {
                    var jsonString = data;
                    dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                    foreach (var symbol in entity.symbols)
                    {
                        if (symbol.status == STATUS_TRADING)
                        {
                            MarketPairs.Add(new MarketPair()
                            {
                                BaseAsset = symbol.baseAsset,
                                QuoteAsset = symbol.quoteAsset,
                                Pair = symbol.symbol,
                                Status = symbol.status
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // TODO: запись лога в БД
                    throw ex;
                }
            }
            else
            {
                throw new ArgumentException("Value IsNullOrWhiteSpace", "data");
            }
        }
    }
}
