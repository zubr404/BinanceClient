using DataBaseWork;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using StockExchenge;
using TestApp.REST;
using System.Threading.Tasks;
using StockExchenge.MarketTrades;

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


            /*UserStreamData userStreamData = new UserStreamData();
            var response = userStreamData.GetListenKeyUserStream();
            Console.WriteLine(response);
            var listenKey = response.Replace("listenKey", "").Replace("{", "").Replace("}", "").Replace("\"", "").Replace(":", "").Replace(" ", "");
            Console.WriteLine(listenKey);
            userStreamData.UserStreamWebSocket(listenKey);*/

            var tradesWs = new CurrentTrades(new string[] { "ETHBTC", "BTCUSDT" });
            tradesWs.SocketOpen();

            tradesWs.ConnectStateEvent += (object sender, string e) =>
            {
                Console.WriteLine(e);
            };
            tradesWs.MessageEvent += (object sender, CurrentTradeEventArgs e) =>
            {
                Console.WriteLine(e.Message);
            };

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
