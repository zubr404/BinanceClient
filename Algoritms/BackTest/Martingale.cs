using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Algoritms.BackTest
{
    public class Martingale
    {
        private readonly TradeHistoryRepository repository;
        public Martingale(TradeHistoryRepository repository)
        {
            this.repository = repository;
        }
        public void StartTest(BackTestConfiguration configuration)
        {

        }

        private List<TradeHistory> GetTades(long startTime, long endTime)
        {
            
        }
    }
}
