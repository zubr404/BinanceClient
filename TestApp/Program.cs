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

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
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

            var dataBase = new DataBaseContext();
            var dataBaseForTrade = new DataBaseContext();
            var aPIKeyRepository = new APIKeyRepository();
            var tradeConfigRepository = new TradeConfigRepository();
            var tradeRepository = new TradeRepository();
            var tradeAccountInfo = new TradeAccountInfo(aPIKeyRepository, tradeConfigRepository, tradeRepository);

            tradeAccountInfo.RequestedTrades("ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO",
                "ZzzmG2XkUiBk4hpoF98u3jnP3R5NN39nU6CS5MW65d3WsyzbtGxGT7bboz2fXVDT", "ETHUSDT");

            Console.WriteLine("FINISH");
            Console.ReadKey();
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
