using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class StopLimitOrder
    {
        public int ID { get; set; }
        public string FK_PublicKey { get; set; } // связь должна быть по публичному ключу
        public string Pair { get; set; }
        public double StopPrice { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public bool IsBuyOperation { get; set; }
        public bool Active { get; set; }
        public APIKey APIKey { get; set; }
    }
}
