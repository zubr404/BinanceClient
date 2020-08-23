using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    /// <summary>
    /// Пара подключенная для торговли
    /// </summary>
    public class ConnectedPair
    {
        public int ID { get; set; }
        public string MainCoin { get; set; }
        public string AltCoin { get; set; }
        public bool Active { get; set; }
    }
}
