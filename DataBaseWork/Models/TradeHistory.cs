using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
	/*
	[
		{
			"id":1,
			"price":"4261.48000000",
			"qty":"1.60000000",
			"quoteQty":"6818.36800000",
			"time":1502942432285,
			"isBuyerMaker":true,
			"isBestMatch":true
		}
	]
     */
	public class TradeHistory
    {
		public int ID { get; set; }
		public long TradeId { get; set; }
		public double Price { get; set; }
		public double Qty { get; set; }
		public double QuoteQty { get; set; }
		public long Time { get; set; }
		public bool IsBuyerMaker { get; set; }
		public bool IsBestMatch { get; set; }
	}
}
