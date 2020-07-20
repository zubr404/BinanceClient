using BinanceClient.Services;
using BinanceClient.ViewModel.Scrin1;
using DataBaseWork;
using DataBaseWork.Repositories;
using StockExchenge.MarketTradesHistory;
using System.Dynamic;
using System.Windows.Media;
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
        public LeftPanelScrin1 LeftPanelScrin1 { get; set; }
        public RightPanelScrin1 RightPanelScrin1 { get; set; }

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            dataBase = new DataBaseContext();
            tradeHistoryRepository = new TradeHistoryRepository(dataBase);
            ConsoleScrin1 = new ConsoleScrin1();
            LeftPanelScrin1 = new LeftPanelScrin1();
            RightPanelScrin1 = new RightPanelScrin1();
            ChartService = new ChartService(dispatcher);
        }

        #region Commands
        // TODO: пара с морды
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

        #region Кнопки левой панели
        private RelayCommand mainButtonCommand;
        public RelayCommand MainButtonCommand
        {
            get
            {
                return mainButtonCommand ?? new RelayCommand((object o) =>
                {
                    LeftPanelScrin1.ManagingBackground(ButtonName.MainButton);
                });
            }
        }
        private RelayCommand currentStatisticButtonCommand;
        public RelayCommand CurrentStatisticButtonCommand
        {
            get
            {
                return currentStatisticButtonCommand ?? new RelayCommand((object o) =>
                {
                    LeftPanelScrin1.ManagingBackground(ButtonName.CurrentStatistic);
                });
            }
        }
        private RelayCommand caltulatorButtonCommand;
        public RelayCommand CaltulatorButtonCommand
        {
            get
            {
                return caltulatorButtonCommand ?? new RelayCommand((object o) =>
                {
                    LeftPanelScrin1.ManagingBackground(ButtonName.Calculator);
                });
            }
        }
        private RelayCommand generalStatisticsButtonCommand;
        public RelayCommand GeneralStatisticsButtonCommand
        {
            get
            {
                return generalStatisticsButtonCommand ?? new RelayCommand((object o) =>
                {
                    LeftPanelScrin1.ManagingBackground(ButtonName.GeneralStatistics);
                });
            }
        }
        private RelayCommand backtestingButtonCommand;
        public RelayCommand BacktestingButtonCommand
        {
            get
            {
                return backtestingButtonCommand ?? new RelayCommand((object o) =>
                {
                    LeftPanelScrin1.ManagingBackground(ButtonName.BackTesting);
                });
            }
        }
        #endregion

        #region Кнопки правой панели
        private RelayCommand btcusd_longCommand;
        public RelayCommand BTCUSD_LONGCommand
        {
            get
            {
                return btcusd_longCommand ?? new RelayCommand((object o) =>
                {
                    RightPanelScrin1.ManagingBackground(ButtonName.BTCUSD_LONG);
                });
            }
        }
        private RelayCommand btcusd_shortCommand;
        public RelayCommand BTCUSD_SHORTCommand
        {
            get
            {
                return btcusd_shortCommand ?? new RelayCommand((object o) =>
                {
                    RightPanelScrin1.ManagingBackground(ButtonName.BTCUSD_SHORT);
                });
            }
        }
        private RelayCommand ethusd_shortCommand;
        public RelayCommand ETHUSD_SHORTCommand
        {
            get
            {
                return ethusd_shortCommand ?? new RelayCommand((object o) =>
                {
                    RightPanelScrin1.ManagingBackground(ButtonName.ETHUSD_SHORT);
                });
            }
        }
        #endregion
        #endregion


        private void TradesHistory_LoadStateEvent(object sender, string e)
        {
            ConsoleScrin1.Message = e;
        }
    }
}
