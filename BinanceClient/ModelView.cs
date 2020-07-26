using BinanceClient.Services;
using BinanceClient.ViewModel.Scrin1;
using BinanceClient.ViewModel.ScrinCalculator;
using DataBaseWork;
using DataBaseWork.Repositories;
using StockExchenge.MarketTradesHistory;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace BinanceClient
{
    class ModelView
    {
        private readonly Dispatcher dispatcher;
        private readonly DataBaseContext dataBase;
        private readonly TradeHistoryRepository tradeHistoryRepository;
        private readonly TradeConfigRepository tradeConfigRepository;
        public ChartService ChartService { get; set; }
        private TradesHistory tradesHistory;

        public ScrinManager ScrinManager { get; set; }
        public static ConsoleScrin1 ConsoleScrin1 { get; private set; }
        public LeftPanelScrin1 LeftPanelScrin1 { get; set; }
        public RightPanelScrin1 RightPanelScrin1 { get; set; }
        public CentralPanelScrin1 CentralPanelScrin1 { get; set; }

        // test
        public List<CalculatingData> CalculatingDatas { get; set; }

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            dataBase = new DataBaseContext();
            tradeHistoryRepository = new TradeHistoryRepository(dataBase);
            tradeConfigRepository = new TradeConfigRepository(dataBase);

            ScrinManager = new ScrinManager();
            ConsoleScrin1 = new ConsoleScrin1();
            LeftPanelScrin1 = new LeftPanelScrin1();
            RightPanelScrin1 = new RightPanelScrin1();
            CentralPanelScrin1 = new CentralPanelScrin1(tradeConfigRepository);
            ChartService = new ChartService(dispatcher);

            CalculatingDatas = new List<CalculatingData>()
            {
                new CalculatingData(){ Amount = "0.001432", Equivalent = "10.00710240", PriceInGrid = "6993.37", ProfitPrice = "6993.37", Rebount = "1.5"},
                new CalculatingData(){  Amount = "0.001432", Equivalent = "10.00710240", PriceInGrid = "6993.37", ProfitPrice = "6993.37", Rebount = "1.5"}
            };
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
                    ScrinManager.ManagingScrin(ScrinName.Scrin1);
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
                    ScrinManager.ManagingScrin(ScrinName.ScrinCurrentStatistic);
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
                    ScrinManager.ManagingScrin(ScrinName.ScrinCalculator);
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
                    ScrinManager.ManagingScrin(ScrinName.ScrinGeneralStatistic);
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
                    ScrinManager.ManagingScrin(ScrinName.ScrinBacktesting);
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
