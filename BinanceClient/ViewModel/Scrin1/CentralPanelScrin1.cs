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
        public TradeConfigurationView TradeConfigurationView { get; private set; }
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
            KeyPanelScrin1 keyPanelScrin1)
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
            SetConfigView();
            // получаем сохраненные конфиги для правых кнопок
        }

        private void SetConfigView()
        {
            var config = configRepository.GetLast();
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
                        accountInfo.RequestedBalances();
                        tradeAccountInfo.RequestedTrades();
                        currentTrades.SocketOpen();
                        userStreamData.StreamStart();
                        martingale.StartAlgoritm();

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
                    martingale.StopAlgoritm();
                    StartButton.IsEnabled = true;
                    ModelView.ConsoleScrin1.Message = "Алгоритм остановлен.";
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
                            configRepository.Update(config);
                            ModelView.ConsoleScrin1.Message = $"Конфигурация для {config.MainCoin}/{config.AltCoin} успешно применена.";
                            martingale.CancelAllStopOrders();
                            martingale.CancelAllTakeProfitOrder();
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

        private RelayCommand apiKeyCommand;
        public RelayCommand ApiKeyCommand
        {
            get
            {
                return apiKeyCommand ?? new RelayCommand((object o) =>
                {
                    keyPanelScrin1.OpenPanel();
                });
            }
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
