using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge.MarketTradesHistory
{
    public class TradeHistoryBuffer
    {
        private const int BUFFER_SIZE = 100000;
        public List<TradeHistory> TradeHistories { get; private set; }
        private readonly TradeHistoryRepository repository;

        public TradeHistoryBuffer(TradeHistoryRepository repository)
        {
            this.repository = repository;
            TradeHistories = new List<TradeHistory>();
        }

        public void AddRange(IEnumerable<TradeHistory> trades)
        {
            if(TradeHistories.Count >= BUFFER_SIZE)
            {
                repository.AddRange(TradeHistories);
                TradeHistories.Clear();
            }
            TradeHistories.AddRange(trades);
        }

        public void Save()
        {
            if(TradeHistories.Count > 0)
            {
                repository.AddRange(TradeHistories);
                TradeHistories.Clear();
            }
        }
    }
}
