using DataBaseWork;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using StockExchenge;
using TestApp.REST;
using System.Threading.Tasks;
using StockExchenge.MarketTrades;
using StockExchenge.StreamWs;
using System.Threading;
using StockExchenge.RestApi;
using System.Text.RegularExpressions;
using Services;
using System.Collections.Generic;
using System.Linq;
using StockExchenge.TradeAccount;
using System.IO;
using TestApp.TradeHistory;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var dtStart = DateTime.Now;
            Console.WriteLine($">>> START {dtStart}");
            //foreach (var interval in KlineType.Intervals)
            //{
            //    Kline kline = new Kline("ethbtc", interval);
            //    kline.SocketOpen();
            //}

            //Kline kline = new Kline("ethbtc", "1m");
            ////kline.SocketOpen("ethbtc", KlineType.d1);
            //Console.WriteLine(kline.GetHistory());

            //Task.Run(() =>
            //{
            //    HttpGetTradeHistory httpGetTradeHistory = new HttpGetTradeHistory();
            //    for (int i = 0; i < 100; i++)
            //    {
            //        if (httpGetTradeHistory.Rquest1().Result == 200)
            //        {
            //            break;
            //        }
            //        else
            //        {
            //            Console.WriteLine(i);
            //        }
            //    }
            //});


            //UserStreamData userStreamData = new UserStreamData();
            //var response = userStreamData.GetListenKeyUserStream();
            //Console.WriteLine(response);
            //var listenKey = response.Replace("listenKey", "").Replace("{", "").Replace("}", "").Replace("\"", "").Replace(":", "").Replace(" ", "");
            //Console.WriteLine(listenKey);
            //userStreamData.UserStreamWebSocket(listenKey);

            //var db = new DataBaseContext();
            //var connectedPairRepo = new ConnectedPairRepository(db);
            //var tradesWs = new CurrentTrades(connectedPairRepo);
            //tradesWs.SocketOpen();

            //tradesWs.ConnectStateEvent += (object sender, string e) =>
            //{
            //    Console.WriteLine(e);
            //};
            //tradesWs.LastPriceEvent += (object sender, LastPriceEventArgs e) =>
            //{
            //    Console.WriteLine($"{e.Pair} - {e.LastPrice}");
            //};


            /*var userStream = new StockExchenge.StreamWs.UserStreamData(new APIKeyRepository(new DataBaseContext()));
            userStream.StreamStart();*/

            //var alg = new Algoritms.Real.Martingale();
            //var result = alg.SendOrder("ETHUSDT", false, 0.02997, "ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO", "ZzzmG2XkUiBk4hpoF98u3jnP3R5NN39nU6CS5MW65d3WsyzbtGxGT7bboz2fXVDT");

            //Console.WriteLine($"{result.OrderId}\n{result.Msg}");

            /*double value = 249.03123456;
            decimal step = 0.00001m;
            Console.WriteLine(value);
            int res = (int)((decimal)value / step);
            decimal roundSize = res * step;
            double result = (double)roundSize;

            Console.WriteLine(result);*/

            //const int countRequest = 10;
            //const int sleepMillisecond = 2000;
            //for (int i = 0; i < countRequest; i++)
            //{
            //    SecretKeyRequiredRequester privateApi = new SecretKeyRequiredRequester();
            //    string response = string.Empty;
            //    Regex my_reg = new Regex(@"\D");
            //    string serverTime = string.Empty;
            //    try
            //    {
            //        serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //    try
            //    {
            //        response = privateApi.GetWebRequest($"{Resources.DOMAIN_V3}myTrades?symbol=ETHUSDT&recvWindow=5000&timestamp={serverTime}", $"symbol=ETHUSDT&recvWindow=5000&timestamp={serverTime}", "ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO", "ZzzmG2XkUiBk4hpoF98u3jnP3R5NN39nU6CS5MW65d3WsyzbtGxGT7bboz2fXVDT", "GET");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //    var list = JConverter.JsonConver<List<Trade>>(response);
            //    Thread.Sleep(sleepMillisecond);
            //    Console.WriteLine(i);
            //}

            //var dataBase = new DataBaseContext();
            //var dataBaseForTrade = new DataBaseContext();
            //var aPIKeyRepository = new APIKeyRepository();
            //var tradeConfigRepository = new TradeConfigRepository();
            //var tradeRepository = new TradeRepository();
            //var tradeAccountInfo = new TradeAccountInfo(aPIKeyRepository, tradeConfigRepository, tradeRepository);

            //tradeAccountInfo.RequestedTrades("ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO",
            //    "ZzzmG2XkUiBk4hpoF98u3jnP3R5NN39nU6CS5MW65d3WsyzbtGxGT7bboz2fXVDT", "ETHUSDT");

            #region поиск даты в истории сделок
            //var dateTradeHistory = new DateTradeHistory();
            //dateTradeHistory.GetDateTrade();

            //long lastDateUnix = 0;
            //for (int i = 1; i <= 5; i++)
            //{
            //    var dateLocal = new DateTime(1970, 1, i).AddHours(3);
            //    Console.WriteLine(dateLocal);

            //    var dateUtc = dateLocal.ToUniversalTime();
            //    Console.WriteLine(dateUtc);

            //    var dateUnix = dateUtc.ToUnixTime();
            //    Console.WriteLine(dateUnix);

            //    if (lastDateUnix > 0)
            //    {
            //        Console.WriteLine(dateUnix - lastDateUnix);
            //    }
            //    lastDateUnix = dateUnix;
            //}

            // определяем номер дня, который мы хотим найти в формате unix
            /*const long daySizeUnix = 86400000; // количество миллисекунд в дне
            var queryDateLocal = new DateTime(2019, 5, 21).AddHours(3); // дата, указанная пользователем
            var queryDateUnix = queryDateLocal.ToUniversalTime().ToUnixTime();
            var numQueryDay = queryDateUnix / daySizeUnix;
            Console.WriteLine($"{numQueryDay}: numQueryDay");

            var numDayFirstTrade = 0L;
            var numDayLastTrade = 0L;
            // получаем дату первой сделки торгов
            var firstTradeTime = GetTimeTrade(GetUrlFromFirstTrade(DateTradeHistory.PAIR), out _);
            if (firstTradeTime > 0)
            {
                numDayFirstTrade = firstTradeTime / daySizeUnix;
                Console.WriteLine($"{numDayFirstTrade}: numDayFirstTrade");
            }

            // получаем дату последней сделки торгов
            var lastTradeTime = GetTimeTrade(GetUrlFromLastTrade(DateTradeHistory.PAIR), out int lastTradeId);
            if (lastTradeTime > 0)
            {
                numDayLastTrade = lastTradeTime / daySizeUnix;
                Console.WriteLine($"{numDayLastTrade}: numDayLastTrade");
            }

            // общее количество торговых дней
            var allTradingDays = numDayLastTrade - numDayFirstTrade;
            Console.WriteLine($"{allTradingDays}: allTradingDays");

            // среднее количество сделок в день
            //var averageTradesDay = lastTradeId / allTradingDays;
            //Console.WriteLine($"{averageTradesDay}: averageTradesDay");

            // *******
            //var countDayBack = numDayLastTrade / daySizeUnix - numQueryDay; // на какое количество дней нужно вернуться
            //var countTradeBack = averageTradesDay * countDayBack; // на какое количество сделок нужно вернуться
            var fromId = lastTradeId - countTradeBack;
            var desiredTimeTrade = GetTimeTrade(GetUrl(DateTradeHistory.PAIR, fromId), out _); // иское время сделки
            Console.WriteLine($"desiredTimeTrade: {desiredTimeTrade.UnixToDateTime()} queryDateLocal: {queryDateLocal.ToUniversalTime()}");

            while (true)
            {
                countDayBack = desiredTimeTrade - numQueryDay; // на какое количество дней нужно вернуться
                countTradeBack = averageTradesDay * countDayBack; // на какое количество сделок нужно вернуться
                var fromId = lastTradeId - countTradeBack;
                desiredTimeTrade = GetTimeTrade(GetUrl(DateTradeHistory.PAIR, fromId), out _); // иское время сделки
                Console.WriteLine($"desiredTimeTrade: {desiredTimeTrade.UnixToDateTime()} queryDateLocal: {queryDateLocal.ToUniversalTime()}");
            }*/

            #endregion

            //TradeDaySearcher.SearchIntTest2(new DateTime(1021, 1, 25));

            //Regex regex = new Regex(@"([1-31]{2}).([1-12]{2}).([1950-2050]{4})");

            //if (regex.Match("21.05.2017").Success)
            //{
            //    Console.WriteLine("Дата верная.");
            //}
            //else
            //{
            //    Console.WriteLine("Дата ne верная.");
            //}

            //var pattern = @"^[0-9]{2}\.[0-9]{2}\.[0-9]{4}$";
            //while (true)
            //{
            //    var dateStr = Console.ReadLine();
            //    if (Regex.IsMatch(dateStr, pattern))
            //    {
            //        Console.WriteLine("Дата верная.");
            //    }
            //    else
            //    {
            //        Console.WriteLine("Дата ne верная.");
            //    }
            //}

            var values1 = new List<long>();
            var values2 = new List<long>();

            const int countI = 100;

            for (int i = 0; i < countI; i++)
            {
                values1.Add(10 + i);
            }
            for (int i = 0; i < countI; i++)
            {
                values2.Add(50 + i);
            }

            var exepts = values1.Except(values2);

            foreach (var item in exepts)
            {
                Console.WriteLine(item);
            }


            var dtFinish = DateTime.Now;
            Console.WriteLine($">>> FINISH {dtFinish} ({dtFinish - dtStart})");
            Console.ReadKey();
        }

        public static void SearchPrototip()
        {
            var twoValueList = new List<TwoValueModel>();
            var lastTime = 1610668800000L;
            var random = new Random();
            for (int i = 0; i <= 100000000; i++)
            {
                twoValueList.Add(new TwoValueModel()
                {
                    ID = i,
                    TimeUnix = i // !!!
                });
                lastTime += random.Next(100, 5001);
            }

            var twoValueTimeFirst = twoValueList.First().TimeUnix;
            var twoValueTimeLast = twoValueList.Last().TimeUnix;
            Console.WriteLine($"{twoValueTimeFirst} <--> {twoValueTimeLast}");

            for (int I = 0; I <= 1000; I++)
            {
                var timeForSearch = I;// twoValueTimeFirst + random.Next(0, 100000);
                                      //Console.WriteLine($"ИСКОМОЕ ЗНАЧЕНИЕ: {timeForSearch}");

                var half = twoValueList.Count() / 2;
                var midleIndex = half;
                var previosMidleIndex = 0L;
                var countStep = 0;
                while (true)
                {
                    countStep++;
                    //Thread.Sleep(500);
                    //Console.WriteLine($"-------------------------------------");

                    //Console.WriteLine($"{previosMidleIndex} -> previosMidleIndex");
                    //Console.WriteLine($"{midleIndex} -> midleIndex");
                    half /= 2;
                    //Console.WriteLine($"{half} -> half");

                    var time = twoValueList[midleIndex].TimeUnix;
                    //Console.WriteLine($"{time} -> time");

                    if (time == timeForSearch)
                    {
                        Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО   : {timeForSearch}\t{countStep}");
                        break;
                    }
                    else if (time < timeForSearch)
                    {
                        previosMidleIndex = midleIndex;
                        midleIndex += half;

                        if (half == 0)
                        {
                            for (int i = midleIndex; i < twoValueList.Count(); i++)
                            {
                                countStep++;
                                time = twoValueList[i].TimeUnix;
                                if (time == timeForSearch)
                                {
                                    Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО UP: {timeForSearch}\t{countStep}");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        previosMidleIndex = midleIndex;
                        midleIndex -= half;

                        if (half == 0)
                        {
                            countStep++;
                            for (int i = midleIndex; i >= 0; i--)
                            {
                                time = twoValueList[i].TimeUnix;
                                if (time == timeForSearch)
                                {
                                    Console.WriteLine($"ЗНАЧЕНИЕ НАЙДЕНО DW: {timeForSearch}\t{countStep}");
                                    break;
                                }
                            }
                        }
                    }

                    if (half == 0)
                    {
                        break;
                    }
                }
            }
        }

        //static void DBtest()
        //{
        //    var usersRepo = new UserRepository(new DataBaseContext());

        //    try
        //    {
        //        var user = usersRepo.Create(new User() { Login = "login" });
        //        usersRepo.Create(new User() { Login = "login1" });
        //        //usersRepo.Update(new User() { ID = 1, Login = "login" });

        //        foreach (var item in usersRepo.Get())
        //        {
        //            Console.WriteLine(item.ID);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"{ex.Message}");
        //    }
        //}
    }
}
