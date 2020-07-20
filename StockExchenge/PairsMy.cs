using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge
{
    /// <summary>
    /// TODO: Выполняет функцию временного тестого хранилища подключенных пар.
    /// Будет перенесено в БД
    /// </summary>
    public class PairsMy
    {
        public static List<string> Pairs = new List<string>()
        {
            "ETHBTC",
            "GNTBTC",
            "BTCUSDT"
        };
    }
}
