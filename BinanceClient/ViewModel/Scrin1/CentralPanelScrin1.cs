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

        public CentralPanelScrin1(AccountInfo accountInfo, TradeAccountInfo tradeAccountInfo, CurrentTrades currentTrades, UserStreamData userStreamData, TradeConfigRepository configRepository)
        {
            TradeConfigurationView = new TradeConfigurationView();
            this.accountInfo = accountInfo;
            this.tradeAccountInfo = tradeAccountInfo;
            this.currentTrades = currentTrades;
            this.userStreamData = userStreamData;
            this.configRepository = configRepository;
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
                TradeConfigurationView.OrderStepPlus = config.OrderStepPlus;
                TradeConfigurationView.Profit = config.Profit;
                TradeConfigurationView.SqueezeProfit = config.SqueezeProfit;
                TradeConfigurationView.Strategy = config.Strategy;
                TradeConfigurationView.TrallingForvard = config.TrallingForvard;
                TradeConfigurationView.TrallingStop = config.TrallingStop;
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ?? new RelayCommand((object o) =>
                {
                    accountInfo.RequestedBalances();
                    tradeAccountInfo.RequestedTrades();
                    currentTrades.SocketOpen();
                    userStreamData.StreamStart();
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
                        SqueezeProfit = TradeConfigurationView.SqueezeProfit,
                        Strategy = TradeConfigurationView.Strategy,
                        TrallingForvard = TradeConfigurationView.TrallingForvard,
                        TrallingStop = TradeConfigurationView.TrallingStop
                    };

                    try
                    {
                        configRepository.Update(config);
                        ModelView.ConsoleScrin1.Message = $"Конфигурация для {config.MainCoin}/{config.AltCoin} успешно применена.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}\n{ex.InnerException?.Message}");
                    }
                });
            }
        }
    }
}
