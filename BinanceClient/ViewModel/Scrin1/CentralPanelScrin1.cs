using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BinanceClient.Models;
using System.Windows;
using BinanceClient.Services;
using StockExchenge.StreamWs;
using StockExchenge.TradeAccount;
using StockExchenge.BalanceAccount;
using StockExchenge.MarketTrades;
using Algoritms.Real;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace BinanceClient.ViewModel.Scrin1
{
    public class CentralPanelScrin1
    {
        public delegate void LoadChart(string value);
        public LoadChart loadChart;

        public TradeConfigurationView TradeConfigurationView { get; private set; }
        readonly RightPanelScrin1 rightPanelScrin1;
        readonly TradeConfigRepository configRepository;
        readonly AccountInfo accountInfo;
        readonly TradeAccountInfo tradeAccountInfo;
        readonly CurrentTrades currentTrades;
        readonly UserStreamData userStreamData;
        readonly Martingale martingale;

        readonly KeyPanelScrin1 keyPanelScrin1;

        public StartButton StartButton { get; set; }
        public StopButton StopButton { get; set; }

        Task task;

        public CentralPanelScrin1(
            AccountInfo accountInfo, 
            TradeAccountInfo tradeAccountInfo, 
            CurrentTrades currentTrades, 
            UserStreamData userStreamData, 
            TradeConfigRepository configRepository,
            Martingale martingale,
            KeyPanelScrin1 keyPanelScrin1,
            RightPanelScrin1 rightPanelScrin1)
        {
            StartButton = new StartButton();
            StopButton = new StopButton();

            TradeConfigurationView = new TradeConfigurationView();
            this.accountInfo = accountInfo;
            this.tradeAccountInfo = tradeAccountInfo;
            this.currentTrades = currentTrades;
            this.userStreamData = userStreamData;
            this.configRepository = configRepository;
            this.martingale = martingale;
            this.keyPanelScrin1 = keyPanelScrin1;
            this.rightPanelScrin1 = rightPanelScrin1;
            SetConfigView();
            SetPairs();
            // получаем сохраненные конфиги для правых кнопок
        }

        private void SetPairs()
        {
            TradeConfigurationView.Coins = MainWindow.ExchangeInfo.AllPairsMarket.MarketPairs.OrderBy(x => x.Pair).Select(x => x.BaseAsset).Distinct().ToList();
            var altCoins = MainWindow.ExchangeInfo.AllPairsMarket.MarketPairs.OrderBy(x => x.Pair).Select(x => x.QuoteAsset);
            var exceptCoins = altCoins.Except(TradeConfigurationView.Coins);
            foreach (var altCoin in exceptCoins)
            {
                TradeConfigurationView.Coins.Add(altCoin);
            }
            TradeConfigurationView.Coins.Sort();
        }

        private void SetConfigView()
        {
            var config = configRepository.GetActive(Resources.SAVED_STRATEGIES);
            if(config != null)
            {
                TradeConfigurationView.AltCoin = config.AltCoin;
                TradeConfigurationView.DepositLimit = config.DepositLimit;
                TradeConfigurationView.FirstStep = config.FirstStep;
                TradeConfigurationView.IntervalHttp = config.IntervalHttp;
                TradeConfigurationView.MainCoin = config.MainCoin;
                TradeConfigurationView.Margin = config.Margin;
                TradeConfigurationView.Martingale = config.Martingale;
                TradeConfigurationView.OpenOrders = config.OpenOrders;
                TradeConfigurationView.OrderDeposit = config.OrderDeposit;
                TradeConfigurationView.OrderIndent = config.OrderIndent;
                TradeConfigurationView.OrderReload = config.OrderReload;
                TradeConfigurationView.OrderStepPlus = config.OrderStepPlus;
                TradeConfigurationView.Profit = config.Profit;
                TradeConfigurationView.ProtectiveSpread = config.ProtectiveSpread;
                TradeConfigurationView.Strategy = config.Strategy;
                TradeConfigurationView.IndentExtremum = config.IndentExtremum;
                TradeConfigurationView.Loss = config.Loss;
                TradeConfigurationView.OrderIndent = config.OrderIndent;
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ?? new RelayCommand((object o) =>
                {
                    task = Task.Run(() =>
                    {
                        try
                        {
                            accountInfo.RequestedBalances();
                            tradeAccountInfo.RequestedTrades(); // это возможно убрать (мы должны иметь сделки только выполненные роботом, иначе мы не узнаем к какому конфигу ее отнести)
                            currentTrades.SocketOpen();
                            userStreamData.StreamStart();
                            martingale.StartAlgoritm();
                        }
                        catch (Exception ex)
                        {
                            ModelView.ConsoleScrin1.Message = $"Exception: {ex.Message}\nInnerException: {ex.InnerException?.Message}\nTrace: {ex.StackTrace}";
                        }
                        

                        #region test takeprofit
                        //var lastPrice = 900.0;
                        //for (int i = 0; i < 10000; i++)
                        //{
                        //    martingale.TakeProfitTraking(lastPrice);

                        //    if (i < 300)
                        //    {
                        //        lastPrice++;
                        //    }
                        //    else if (i >= 300 && i < 600)
                        //    {
                        //        lastPrice--;
                        //    }
                        //}
                        #endregion
                    });
                    StartButton.IsEnabled = false;
                    ModelView.ConsoleScrin1.Message = "Алгоритм запущен.";
                });
            }
        }

        private RelayCommand stopCommand;
        public RelayCommand StopCommand
        {
            get
            {
                return stopCommand ?? new RelayCommand((object o) => 
                {
                    if (MessageBox.Show("Будут сняты все активные Заявки. Продолжить?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        martingale.StopAlgoritm();
                        StartButton.IsEnabled = true;
                        ModelView.ConsoleScrin1.Message = "Алгоритм остановлен.";
                    }
                });
            }
        }

        private RelayCommand applyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                return applyCommand ?? new RelayCommand((object o) =>
                {
                    var config = new TradeConfiguration()
                    {
                        Active = true,
                        AltCoin = TradeConfigurationView.AltCoin,
                        DepositLimit = TradeConfigurationView.DepositLimit,
                        FirstStep = TradeConfigurationView.FirstStep,
                        IntervalHttp = TradeConfigurationView.IntervalHttp,
                        MainCoin = TradeConfigurationView.MainCoin,
                        Margin = TradeConfigurationView.Margin,
                        Martingale = TradeConfigurationView.Martingale,
                        OpenOrders = TradeConfigurationView.OpenOrders,
                        OrderDeposit = TradeConfigurationView.OrderDeposit,
                        OrderIndent = TradeConfigurationView.OrderIndent,
                        OrderStepPlus = TradeConfigurationView.OrderStepPlus,
                        Profit = TradeConfigurationView.Profit,
                        ProtectiveSpread = TradeConfigurationView.ProtectiveSpread,
                        Strategy = TradeConfigurationView.Strategy,
                        IndentExtremum = TradeConfigurationView.IndentExtremum,
                        Loss = TradeConfigurationView.Loss,
                        OrderReload = TradeConfigurationView.OrderReload
                    };

                    if(MessageBox.Show("Будут сняты все активные Заявки. Продолжить?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            configRepository.Update(config, Resources.SAVED_STRATEGIES);
                            ModelView.ConsoleScrin1.Message = $"Конфигурация для {config.MainCoin}/{config.AltCoin} {config.Strategy} успешно применена.";
                            martingale.CancelAllStopOrders();
                            martingale.CancelAllTakeProfitOrder();

                            loadChart?.Invoke(TradeConfigurationView.MainCoin + TradeConfigurationView.AltCoin);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}\n{ex.InnerException?.Message}");
                        }
                    }
                    else
                    {
                        SetConfigView();
                    }
                });
            }
        }

        #region Кнопки правой панели
        // -------------------Включить настройки-----------------------------
        private RelayCommand btcusd_longCommand;
        public RelayCommand BTCUSD_LONGCommand
        {
            get
            {
                return btcusd_longCommand ?? new RelayCommand((object o) =>
                {
                    rightPanelScrin1.ManagingBackground(ButtonName.BTCUSD_LONG);
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
                    rightPanelScrin1.ManagingBackground(ButtonName.BTCUSD_SHORT);
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
                    rightPanelScrin1.ManagingBackground(ButtonName.ETHUSD_SHORT);
                });
            }
        }

        // -------------------Показать настройки-----------------------------
        private RelayCommand btcusd_longShowCommand;
        public RelayCommand BTCUSD_LONGShowCommand
        {
            get
            {
                return btcusd_longShowCommand ?? new RelayCommand((object o) =>
                {
                    var config = configRepository.Get("BTC", "USDT", "long");
                    if (config != null)
                    {
                        SetConfigView(config);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
        }
        private RelayCommand btcusd_shortShowCommand;
        public RelayCommand BTCUSD_SHORTShowCommand
        {
            get
            {
                return btcusd_shortShowCommand ?? new RelayCommand((object o) =>
                {
                    var config = configRepository.Get("BTC", "USDT", "short");
                    if (config != null)
                    {
                        SetConfigView(config);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
        }
        private RelayCommand ethusd_shortShowCommand;
        public RelayCommand ETHUSD_SHORTShowCommand
        {
            get
            {
                return ethusd_shortShowCommand ?? new RelayCommand((object o) =>
                {
                    var config = configRepository.Get("ETH", "USDT", "short");
                    if (config != null)
                    {
                        SetConfigView(config);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
        }
        private RelayCommand mainConfigShowCommand;
        public RelayCommand MainConfigShowCommand
        {
            get
            {
                return mainConfigShowCommand ?? new RelayCommand((object o) =>
                {
                    SetConfigView();
                });
            }
        }
        #endregion

        private void SetConfigView(TradeConfiguration config)
        {
            TradeConfigurationView.AltCoin = config.AltCoin;
            TradeConfigurationView.DepositLimit = config.DepositLimit;
            TradeConfigurationView.FirstStep = config.FirstStep;
            TradeConfigurationView.IntervalHttp = config.IntervalHttp;
            TradeConfigurationView.MainCoin = config.MainCoin;
            TradeConfigurationView.Margin = config.Margin;
            TradeConfigurationView.Martingale = config.Martingale;
            TradeConfigurationView.OpenOrders = config.OpenOrders;
            TradeConfigurationView.OrderDeposit = config.OrderDeposit;
            TradeConfigurationView.OrderIndent = config.OrderIndent;
            TradeConfigurationView.OrderStepPlus = config.OrderStepPlus;
            TradeConfigurationView.Profit = config.Profit;
            TradeConfigurationView.ProtectiveSpread = config.ProtectiveSpread;
            TradeConfigurationView.Strategy = config.Strategy;
            TradeConfigurationView.IndentExtremum = config.IndentExtremum;
            TradeConfigurationView.Loss = config.Loss;
            TradeConfigurationView.OrderReload = config.OrderReload;
        }
    }

    public class StartButton : PropertyChangedBase
    {
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                base.NotifyPropertyChanged();
            }
        }
    }
    public class StopButton : PropertyChangedBase
    {
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
