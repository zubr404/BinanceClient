using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketTrades
{
    #region @aggTrade
    /*{ 
      "e" : "aggTrade" ,   // Тип события 
      "E" : 123456789 ,    // Время события 
      "s" : "BNBBTC" ,     // Символ 
      "a" : 12345 ,        // Совокупный идентификатор сделки 
      "p" : " 0.001 " ,      // Цена 
      " q " : " 100 " ,        // Количество 
      " f " : 100 ,          // Идентификатор первой сделки 
      " l " : 105 ,          // Идентификатор последней сделки 
      " T " :123456785 ,    // Время торговли 
      "м": true ,         // Маркетмейкер - покупатель? 
      "M" : true          // Игнорировать 
        }*/
    #endregion
    public class AggTrade
    {
        public string e { get; set; }
        public string E { get; set; }
        public string s { get; set; }
        public string a { get; set; }
        public string p { get; set; }
        public string q { get; set; }
        public string f { get; set; }
        public string l { get; set; }
        public string T { get; set; }
        public string m { get; set; }
        public string M { get; set; }
    }
}
