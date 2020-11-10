using BinanceClient.Services;
using BinanceClient.ViewModel.Scrin1;
using BinanceClient.ViewModel.ScrinCalculator;
using DataBaseWork;
using DataBaseWork.Repositories;
using StockExchenge.BalanceAccount;
using StockExchenge.MarketTrades;
using StockExchenge.MarketTradesHistory;
using StockExchenge.StreamWs;
using StockExchenge.TradeAccount;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace BinanceClient
{
    class ModelView
    {
        private readonly Dispatcher dispatcher;
        //private readonly APIKeyRepository aPIKeyRepository;
        //private readonly BalanceRepository balanceRepository;
        //private readonly TradeRepository tradeRepository;
        //private readonly TradeHistoryRepository tradeHistoryRepository;
        //private readonly TradeConfigRepository tradeConfigRepository;
        //private readonly ConnectedPairRepository connectedPairRepository;
        //private readonly StopLimitOrderRepository stopLimitOrderRepository;
        //private readonly TakeProfitOrderRepository takeProfitOrderRepository;
        public ChartService ChartService { get; private set; }
        private readonly Algoritms.Real.Martingale martingaleReal;
        private readonly Algoritms.BackTest.Martingale martingaleBackTest;
        private readonly Algoritms.BackTest.CurrentGridStatistics currentGridStatistics;

        private readonly AccountInfo accountInfo;
        private readonly TradeAccountInfo tradeAccountInfo;
        private readonly CurrentTrades currentTrades;
        private readonly UserStreamData userStreamData;
        

        public ScrinManager ScrinManager { get; private set; }
        public static ConsoleScrin1 ConsoleScrin1 { get; private set; }
        public KeyPanelScrin1 KeyPanelScrin1 { get; private set; }
        public LeftPanelScrin1 LeftPanelScrin1 { get; private set; }
        public RightPanelScrin1 RightPanelScrin1 { get; private set; }
        public CentralPanelScrin1 CentralPanelScrin1 { get; private set; }
        public PairPanelScrin1 PairPanelScrin1 { get; private set; }

        public CentralPanelScrinCalculator CentralPanelScrinCalculator { get; set; }

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            //balanceRepository = new BalanceRepository(InitializeDataBase());
            //aPIKeyRepository = new APIKeyRepository(InitializeDataBase());
            //tradeRepository = new TradeRepository(InitializeDataBase());
            //tradeHistoryRepository = new TradeHistoryRepository(InitializeDataBase());
            //tradeConfigRepository = new TradeConfigRepository(InitializeDataBase());
            //connectedPairRepository = new ConnectedPairRepository(InitializeDataBase());
            //stopLimitOrderRepository = new StopLimitOrderRepository(InitializeDataBase());
            //takeProfitOrderRepository = new TakeProfitOrderRepository(InitializeDataBase());

            var connectedPairService = new ConnectedPairService(new ConnectedPairRepository(InitializeDataBase()));
            connectedPairService.InitializeConnectedPair(MainWindow.ExchangeInfo.AllPairsMarket.MarketPairs);

            accountInfo = new AccountInfo(new APIKeyRepository(InitializeDataBase()), new BalanceRepository(InitializeDataBase()));
            tradeAccountInfo = new TradeAccountInfo(new APIKeyRepository(InitializeDataBase()), new TradeConfigRepository(InitializeDataBase()), new TradeRepository(InitializeDataBase()));
            currentTrades = new CurrentTrades(new ConnectedPairRepository(InitializeDataBase()));
            userStreamData = new UserStreamData(new StockExchenge.RepositoriesModel()
            {
                APIKeyRepository = new APIKeyRepository(InitializeDataBase()),
                BalanceRepository = new BalanceRepository(InitializeDataBase())
            });

            martingaleReal = new Algoritms.Real.Martingale(new Algoritms.Real.RepositoriesM()
            {
                APIKeyRepository = new APIKeyRepository(InitializeDataBase()),
                BalanceRepository = new BalanceRepository(InitializeDataBase()),
                CurrentTrades = currentTrades,
                ExchangeInfo = MainWindow.ExchangeInfo,
                TradeConfigRepository = new TradeConfigRepository(InitializeDataBase()),
                StopLimitOrderRepository = new StopLimitOrderRepository(InitializeDataBase()),
                TakeProfitOrderRepository = new TakeProfitOrderRepository(InitializeDataBase()),
                TradeRepository = new TradeRepository(InitializeDataBase()),
                TradeAccountInfo = tradeAccountInfo
            });
            martingaleReal.MessageErrorEvent += MartingaleReal_MessageErrorEvent;
            martingaleReal.MessageDebugEvent += MartingaleReal_MessageDebugEvent;

            martingaleBackTest = new Algoritms.BackTest.Martingale(new TradeHistoryRepository(InitializeDataBase()), new TradeConfigRepository(InitializeDataBase()), MainWindow.ExchangeInfo);
            currentGridStatistics = new Algoritms.BackTest.CurrentGridStatistics(new TradeConfigRepository(InitializeDataBase()));

            ScrinManager = new ScrinManager();
            ConsoleScrin1 = new ConsoleScrin1();
            KeyPanelScrin1 = new KeyPanelScrin1(new APIKeyRepository(InitializeDataBase()), new BalanceRepository(InitializeDataBase()));
            LeftPanelScrin1 = new LeftPanelScrin1();
            RightPanelScrin1 = new RightPanelScrin1();
            CentralPanelScrin1 = new CentralPanelScrin1(accountInfo, tradeAccountInfo, currentTrades, userStreamData, new TradeConfigRepository(InitializeDataBase()), martingaleReal, KeyPanelScrin1, RightPanelScrin1);
            PairPanelScrin1 = new PairPanelScrin1(new ConnectedPairRepository(InitializeDataBase()));
            ChartService = new ChartService(dispatcher);
            CentralPanelScrinCalculator = new CentralPanelScrinCalculator(martingaleBackTest, currentGridStatistics, new TradeHistoryRepository(InitializeDataBase()));

            ChartService.LoadChart(CentralPanelScrin1.TradeConfigurationView.MainCoin + CentralPanelScrin1.TradeConfigurationView.AltCoin);
            CentralPanelScrin1.loadChart = ChartService.LoadChart;
        }

        private DataBaseContext InitializeDataBase()
        {
            try
            {
                return new DataBaseContext();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n{ex.InnerException?.Message}", "ERROR DATABASE", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void MartingaleReal_MessageDebugEvent(object sender, string e)
        {
            ConsoleScrin1.Message = e;
        }

        // выводим ошибки алгоритма
        private void MartingaleReal_MessageErrorEvent(object sender, string e)
        {
            ConsoleScrin1.Message = e;
        }

        #region Commands

        #region Кнопки верхней панели (плохо: кнопки Старт, Стоп, Аппли, АпиКей в классе CentralPanelScrin1)
        private RelayCommand pairCommand;
        public RelayCommand PairCommand
        {
            get
            {
                return pairCommand ?? new RelayCommand((object o) =>
                {
                    ScrinManager.ManagingScrin(ScrinName.ScrinPairConnected);
                    PairPanelScrin1.GetPairs();
                });
            }
        }

        private RelayCommand apiKeyCommand;
        public RelayCommand ApiKeyCommand
        {
            get
            {
                return apiKeyCommand ?? new RelayCommand((object o) =>
                {
                    ScrinManager.ManagingScrin(ScrinName.ScrinKeyManager);
                    KeyPanelScrin1.SetKeys();
                });
            }
        }
        #endregion

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

        #endregion
    }
}
