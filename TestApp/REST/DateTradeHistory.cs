using Services;
using StockExchenge;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestApp.REST
{
    class DateTradeHistory
    {
        public const string PAIR = "BTCUSDT";
        public const long LIMIT = 1000;
        public void GetDateTrade()
        {
            var requester = new PublicKeyRequiredRequester();
            var fromIdStart = 0L;
            var isFoundtrades = true;

            while (isFoundtrades)
            {
                var response = requester.Request(GetUrl(PAIR, fromIdStart), Resources.PUBLIC_KEY);
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    string s = sr.ReadToEnd();

                    List<StockExchenge.MarketTradesHistory.Trade> trades = null;
                    try
                    {
                        trades = JConverter.JsonConver<List<StockExchenge.MarketTradesHistory.Trade>>(s);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Response: {s}");
                        break;
                    }

                    if(trades?.Count > 0)
                    {
                        var tradeFirst = trades.First();
                        var tradesLast = trades.Last();

                        Console.WriteLine($"STSRT TIME {tradeFirst.ID}: {tradeFirst.Time.UnixToDateTime()}");
                        Console.WriteLine($"FINISH TIME {tradesLast.ID}: {tradesLast.Time.UnixToDateTime()}");
                    }
                    else
                    {
                        Console.WriteLine($"NOT FOUND");
                        isFoundtrades = false;
                    }
                    
                }
                fromIdStart += LIMIT;
                Thread.Sleep(1000);
            }
        }
        private string GetUrl(string pair, long fromId)
        {
            return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId={fromId}&limit=1000";
        }
    }
}
