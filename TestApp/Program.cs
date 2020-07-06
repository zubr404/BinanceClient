﻿using DataBaseWork;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using StockExchenge;

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

            Kline kline = new Kline();
            //kline.SocketOpen("ethbtc", KlineType.d1);
            Console.WriteLine(kline.GetHistory("ethbtc", "1m"));



            Console.WriteLine("FINISH");
            Console.ReadKey();
        }

        static void DBtest()
        {
            var usersRepo = new UserRepository(new DataBaseContext());

            try
            {
                var user = usersRepo.Create(new User() { Login = "login" });
                usersRepo.Create(new User() { Login = "login1" });
                //usersRepo.Update(new User() { ID = 1, Login = "login" });

                foreach (var item in usersRepo.Get())
                {
                    Console.WriteLine(item.ID);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
