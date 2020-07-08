using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace StockExchenge
{
    /// <summary>
    /// Выполняет функцию временного тестого хранилища таймфреймов.
    /// Будет перенесено в БД
    /// </summary>
    public class KlineType
    {
        // 1m,3m,5м,15м,30м,1ч,2h,4h,6h,8h,12h,1d,3d,1w,1M
        public const string m1 = "1m";
        public const string m3 = "3m";
        public const string m5 = "5m";
        public const string m15 = "15m";
        public const string m30 = "30m";
        public const string h1 = "1h";
        public const string h2 = "2h";
        public const string h4 = "4h";
        public const string h6 = "6h";
        public const string h8 = "8h";
        public const string h12 = "12h";
        public const string d1 = "1d";
        public const string d3 = "3d";
        public const string w1 = "1w";
        public const string M1 = "1M";

        public static readonly ReadOnlyCollection<string> Intervals = new ReadOnlyCollection<string>(new List<string>()
        {
            "1m",
            "3m",
            "5m",
            "15m",
            "30m",
            "1h",
            "2h",
            "4h",
            "6h",
            "8h",
            "12h",
            "1d",
            "3d",
            "1w",
            "1M"
        });
    }
}
