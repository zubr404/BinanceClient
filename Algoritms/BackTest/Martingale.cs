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
            var startTradeId = repository.MinId(configuration.GeneralSetting.StartTime);
            var stopTradeId = repository.MaxId(configuration.GeneralSetting.StopTime);
            var amountElementsGet = 100000L;

            for (long i = startTradeId; i < stopTradeId; i+=amountElementsGet)
            {
                var simbol = CreateSimbolPair(configuration.MainCoin, configuration.AltCoin);
                var trades = GetTades(simbol, i, i + amountElementsGet);

                foreach (var trade in trades) // запуск теста. проходим по сделкам
                {

                }
            }
        }



        private IEnumerable<TradeHistory> GetTades(string pair, long startTradeId, long stopTradeId)
        {
            return repository.Get(pair, startTradeId, stopTradeId);
        }

        private string CreateSimbolPair(string baseCoin, string altCoin)
        {
            return $"{baseCoin}{altCoin}".ToUpper();
        }
    }
}
