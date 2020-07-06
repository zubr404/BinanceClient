using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.Services
{
    /*
     {
          "e": "kline",     // Тип события
          "E": 123456789,   // Время события
          "s": "BNBBTC",    // пара
          "k": {
            "t": 123400000, // Время открытия свечи (UNIXTIME)
            "T": 123460000, // Время закрытия свечи
            "s": "BNBBTC",  // Пара
            "i": "1m",      // Период
            "f": 100,       // ID первой сделки периода
            "L": 200,       // ID последней сделки периода
            "o": "0.0010",  // Цена открытия
            "c": "0.0020",  // Цена закрытия
            "h": "0.0025",  // High
            "l": "0.0015",  // Low
            "v": "1000",    // Объем базовой валюты
            "n": 100,       // Кол-во сделок
            "x": false,     // Закрыта ли свеча?
            "q": "1.0000",  // Объем квотируемой валюты
            "V": "500",     // Сколько базовой валюты куплено тейкерами
            "Q": "0.500",   // Сколько квотируемой валюты куплено тейкерами
            "B": "123456"   // Не актуально
          }
        }
     */
    class Candle
    {
        /// <summary>
        /// Тип события
        /// </summary>
        public string e { get; set; }
        /// <summary>
        /// Время события
        /// </summary>
        public string E { get; set; }
        /// <summary>
        /// пара
        /// </summary>
        public string s { get; set; }
        public K k { get; set; }

    }

    class K
    {
        /// <summary>
        /// Время открытия свечи (UNIXTIME)
        /// </summary>
        public long t { get; set; }
        /// <summary>
        /// Время закрытия свечи
        /// </summary>
        public long T { get; set; }
        /// <summary>
        /// Пара
        /// </summary>
        public string s { get; set; }
        /// <summary>
        /// Период
        /// </summary>
        public string i { get; set; }
        /// <summary>
        /// ID первой сделки периода
        /// </summary>
        public long f { get; set; }
        /// <summary>
        /// ID последней сделки периода
        /// </summary>
        public long L { get; set; }
        /// <summary>
        /// Цена открытия
        /// </summary>
        public double o { get; set; }
        /// <summary>
        /// Цена закрытия
        /// </summary>
        public double c { get; set; }
        /// <summary>
        /// High
        /// </summary>
        public double h { get; set; }
        /// <summary>
        /// Low
        /// </summary>
        public double l { get; set; }
        /// <summary>
        /// Объем базовой валюты
        /// </summary>
        public double v { get; set; }
        /// <summary>
        /// Кол-во сделок
        /// </summary>
        public int n { get; set; }
        /// <summary>
        /// Закрыта ли свеча?
        /// </summary>
        public bool x { get; set; }
        /// <summary>
        /// Объем квотируемой валюты
        /// </summary>
        public double q { get; set; }
        /// <summary>
        /// Сколько базовой валюты куплено тейкерами
        /// </summary>
        public double V { get; set; }
        /// <summary>
        /// Сколько квотируемой валюты куплено тейкерами
        /// </summary>
        public double Q { get; set; }
    }

    /*
     
     */
}
