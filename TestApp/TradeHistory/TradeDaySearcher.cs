using Services;
using StockExchenge;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestApp.TradeHistory
{
    class TradeDaySearcher
    {
        public const string PAIR = "BTCUSDT";

        [Obsolete]
        public static void SearchIntTest1()
        {
            Console.WriteLine();
            var valueList = new List<long>();
            for (long i = 1; i <= 1002; i++)
            {
                valueList.Add(i);
            }
            Console.WriteLine();

            var foundValue = 1L;
            Console.WriteLine($"foundValue: {foundValue}");

            var downValue = valueList.First();
            var upValue = valueList.Last();
            while (true)
            {
                Thread.Sleep(1000);
                var midleValue = downValue + (upValue - downValue + 1) / 2; // !!!
                Console.WriteLine($"midleValue: {midleValue}");

                if (midleValue == foundValue)
                {
                    Console.WriteLine($"Значение найдено: {foundValue}");
                    break;
                }
                else if (midleValue > foundValue)
                {
                    upValue = midleValue;
                }
                else if (midleValue < foundValue)
                {
                    downValue = midleValue;
                }
            }
        }

        /// <summary>
        /// Поиск стартовой даты скачивания
        /// </summary>
        /// <param name="queryDateLocal">Московское время, указанное пользователем</param>
        public static void SearchIntTest2(DateTime queryDateLocal)
        {
            var queryDateLocalUnix = queryDateLocal.ToUnixTime();

            if (queryDateLocal > DateTime.Now)
            {
                Console.WriteLine("ОШИБКА: ВВЕДЕНА ДАТА ИЗ БУДУЩЕГО!");
                return;
            }

            var firstTime = GetTimeTrade(GetUrlFromFirstTrade(PAIR), out _);
            if(queryDateLocalUnix < firstTime)
            {
                queryDateLocalUnix = firstTime + 1;
                Console.WriteLine($"ОШИБКА: НАЧАЛО ТОРГОВ {firstTime.UnixToDateTime()}");
                Console.WriteLine($"СТАРТОВАЯ ДАТА СКАЧИВАНИЯ = {queryDateLocalUnix.UnixToDateTime()}");
                Console.WriteLine($"ДЛЯ ПРОДОЛЖЕНИЯ НАЖМИТЕ ЛЮБУЮ КЛАВИШУ");
                Console.ReadKey();
                Console.WriteLine();
            }

            // ID последней сделки
            GetTimeTrade(GetUrlFromLastTrade(PAIR), out int lastTradeId);

            var half = lastTradeId / 2;
            var midleIndex = half;
            var countStep = 0;
            while (true)
            {
                countStep++;
                half /= 2;
                var searchTimeUnix = GetTimeTrade(GetUrl(PAIR, midleIndex), out _);
                Console.WriteLine($"(1) {queryDateLocal} <-> {searchTimeUnix.UnixToDateTime()}");

                if (searchTimeUnix == queryDateLocalUnix)
                {
                    Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО   : {queryDateLocalUnix}\t{countStep}");
                    return;
                }
                else if (searchTimeUnix < queryDateLocalUnix)
                {
                    midleIndex += half;
                    if (half == 0)
                    {
                        for (int i = midleIndex; i <= lastTradeId; i++)
                        {
                            countStep++;
                            searchTimeUnix = GetTimeTrade(GetUrl(PAIR, i), out _);
                            Console.WriteLine($"(2) {queryDateLocal} <-> {searchTimeUnix.UnixToDateTime()}");
                            if (searchTimeUnix >= queryDateLocalUnix)
                            {
                                Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО UP: {queryDateLocalUnix}\t{countStep}");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    midleIndex -= half;
                    if (half == 0)
                    {
                        countStep++;
                        for (int i = midleIndex; i >= 0; i--)
                        {
                            searchTimeUnix = GetTimeTrade(GetUrl(PAIR, i), out _);
                            Console.WriteLine($"(3) {queryDateLocal} <-> {searchTimeUnix.UnixToDateTime()}");
                            if (searchTimeUnix <= queryDateLocalUnix)
                            {
                                Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО DW: {queryDateLocalUnix}\t{countStep}");
                                return;
                            }
                        }
                    }
                }
                if (half == 0)
                {
                    Console.WriteLine($"ЗНАЧЕНИЕ HE НАЙДЕНО!!!");
                    break;
                }
            }
        }

        //------------------ helper methods -------------------
        private static long GetTimeTrade(string url, out int id)
        {
            Thread.Sleep(100);
            var requester = new PublicKeyRequiredRequester();
            var response = requester.Request(url, Resources.PUBLIC_KEY);
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
                }

                if (trades?.Count > 0)
                {
                    var tradeFirst = trades.First();
                    id = tradeFirst.ID;
                    return tradeFirst.Time;
                }
                else
                {
                    Console.WriteLine($"NOT FOUND");
                }
                id = 0;
                return 0;
            }
        }
        private static string GetUrlFromFirstTrade(string pair)
        {
            return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId=0&limit=1";
        }
        private static string GetUrlFromLastTrade(string pair)
        {
            return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&limit=1";
        }
        private static string GetUrl(string pair, long fromId)
        {
            return $"{Resources.DOMAIN_V3}historicalTrades?symbol={pair}&fromId={fromId}&limit=1";
        }
    }

    class TwoValueModel
    {
        public long ID { get; set; }
        public long TimeUnix { get; set; }
    }
}
