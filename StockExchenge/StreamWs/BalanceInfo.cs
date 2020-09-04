using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.StreamWs
{
    #region Описание
    /*
     {
      "e": "outboundAccountPosition", //Event type
      "E": 1564034571105,             //Event Time
      "u": 1564034571073,             //Time of last account update
      "B": [                          //Balances Array
                {
                  "a": "ETH",                 //Asset
                  "f": "10000.000000",        //Free
                  "l": "0.000000"             //Locked
                }
            ]
      }
     */
    #endregion
    public class BalanceInfo
    {
        public string e { get; set; }
        public long E { get; set; }
        public long u { get; set; }
        public Balance[] B { get; set; }
    }

    public class Balance
    {
        public string a { get; set; }
        public double f { get; set; }
        public double l { get; set; }
    }
}
