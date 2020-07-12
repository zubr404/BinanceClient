using BinanceClient.Services;
using BinanceClient.ViewModel;
using DataBaseWork;
using DataBaseWork.Repositories;
using StockExchenge;
using StockExchenge.MarketTradesHistory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace BinanceClient
{
    class ModelView
    {
        private readonly Dispatcher dispatcher;
        private readonly DataBaseContext dataBase;
        private readonly TradeHistoryRepository tradeHistoryRepository;
        public ChartService ChartService { get; set; }
        private TradesHistory tradesHistory;

        public static ConsoleScrin1 ConsoleScrin1 { get; private set; }

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            dataBase = new DataBaseContext();
            tradeHistoryRepository = new TradeHistoryRepository(dataBase);
            ConsoleScrin1 = new ConsoleScrin1();
            ChartService = new ChartService(dispatcher);
        }

        #region Commands
        private RelayCommand tradeHistoryLoad;
        public RelayCommand TradeHistoryLoad
        {
            get
            {
                return tradeHistoryLoad ?? new RelayCommand((object o) =>
                {
                    tradesHistory = new TradesHistory("ethbtc", tradeHistoryRepository);
                    tradesHistory.LoadStateEvent += TradesHistory_LoadStateEvent;
                    var result = tradesHistory.Load();
                });
            }
        }
        #endregion


        private void TradesHistory_LoadStateEvent(object sender, string e)
        {
            ConsoleScrin1.Message = e;
        }
    }
}
