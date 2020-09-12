using Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketSettings
{
    public class AllPairsMarket
    {
        public List<MarketPair> MarketPairs { get; private set; }

        public AllPairsMarket(string data)
        {
            MarketPairs = new List<MarketPair>();
            GetPairs(data);
        }

        private void GetPairs(string data)
        {
            try
            {
                var jsonString = data;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    //if (symbol.symbol == currentPair.Pair)
                    //{
                    //    quotePrecision = symbol.quotePrecision;
                    //    basePrecision = symbol.baseAssetPrecision;
                    //    break;
                    //}
                }
            }
            catch (Exception ex)
            {
                // TODO: запись лога в БД
            }
        }
    }
}
