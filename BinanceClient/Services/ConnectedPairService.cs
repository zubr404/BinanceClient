using DataBaseWork.Repositories;
using StockExchenge.MarketSettings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataBaseWork.Models;

namespace BinanceClient.Services
{
    public class ConnectedPairService
    {
        readonly ConnectedPairRepository connectedPairRepository;

        public ConnectedPairService(ConnectedPairRepository connectedPairRepository)
        {
            this.connectedPairRepository = connectedPairRepository;
        }

        public void InitializeConnectedPair(IEnumerable<MarketPair> marketPairs)
        {
            var exchangePairs = marketPairs.Select(x => new ConnectedPair
            {
                MainCoin = x.BaseAsset,
                AltCoin = x.QuoteAsset,
                Active = false
            });

            var dbPairs = connectedPairRepository.Get().Select(x => new ConnectedPair
            {
                MainCoin = x.MainCoin,
                AltCoin = x.AltCoin,
                Active = false
            });

            var exeptPairs = exchangePairs.Except(dbPairs, new ConnectedPairComparer());
            connectedPairRepository.Create(exeptPairs);
        }
    }
}
