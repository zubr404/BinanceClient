using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.Transaction
{
    public class OrderResponse
    {
        public string Symbol { get; set; }
        public string OrderId { get; set; }
        public string ClientOrderId { get; set; }
        public long TransactTime { get; set; }
        public string Msg { get; set; } // заполняется в случае ошибки
    }
}
