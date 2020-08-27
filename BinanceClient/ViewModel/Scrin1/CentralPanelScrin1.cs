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

namespace BinanceClient.ViewModel.Scrin1
{
    public class CentralPanelScrin1
    {
        public TradeConfigurationView TradeConfiguration { get; private set; }
        readonly TradeConfigRepository configRepository;
        readonly APIKeyRepository aPIKeyRepository;
        readonly BalanceRepository balanceRepository;

        public CentralPanelScrin1(APIKeyRepository aPIKeyRepository, BalanceRepository balanceRepository, TradeConfigRepository configRepository)
        {
            TradeConfiguration = new TradeConfigurationView();
            this.aPIKeyRepository = aPIKeyRepository;
            this.balanceRepository = balanceRepository;
            this.configRepository = configRepository;
            SetConfigView();
            // получаем сохраненные конфиги для правых кнопок
        }

        private void SetConfigView()
        {
            var config = configRepository.GetLast();
            if(config != null)
            {
                TradeConfiguration.AltCoin = config.AltCoin;
                TradeConfiguration.DepositLimit = config.DepositLimit;
                TradeConfiguration.FirstStep = config.FirstStep;
                TradeConfiguration.IntervalHttp = config.IntervalHttp;
                TradeConfiguration.MainCoin = config.MainCoin;
                TradeConfiguration.Margin = config.Margin;
                TradeConfiguration.Martingale = config.Martingale;
                TradeConfiguration.OpenOrders = config.OpenOrders;
                TradeConfiguration.OrderDeposit = config.OrderDeposit;
                TradeConfiguration.OrderIndent = config.OrderIndent;
                TradeConfiguration.OrderStepPlus = config.OrderStepPlus;
                TradeConfiguration.Profit = config.Profit;
                TradeConfiguration.SqueezeProfit = config.SqueezeProfit;
                TradeConfiguration.Strategy = config.Strategy;
                TradeConfiguration.TrallingForvard = config.TrallingForvard;
                TradeConfiguration.TrallingStop = config.TrallingStop;
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ?? new RelayCommand((object o) =>
                {
                    // старт торговли
                    // запуск сокетов и др.

                    // test balance
                    //var balanceService = new BalanceService(new StockExchenge.BalanceAccount.AccountInfo(aPIKeyRepository, balanceRepository));
                    //balanceService.GetBalance();

                    //var userstreamData = new UserStreamData(new StockExchenge.RepositoriesModel()
                    //{
                    //    APIKeyRepository = aPIKeyRepository,
                    //    BalanceRepository = balanceRepository
                    //});
                    //userstreamData.StreamStart();
                    // ----
                    var tradeaccountinfo = new TradeAccountInfo(aPIKeyRepository, configRepository);
                    tradeaccountinfo.RequestedTrades();
                    // test TradeAccount


                    //-------
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
                        AltCoin = TradeConfiguration.AltCoin,
                        DepositLimit = TradeConfiguration.DepositLimit,
                        FirstStep = TradeConfiguration.FirstStep,
                        IntervalHttp = TradeConfiguration.IntervalHttp,
                        MainCoin = TradeConfiguration.MainCoin,
                        Margin = TradeConfiguration.Margin,
                        Martingale = TradeConfiguration.Martingale,
                        OpenOrders = TradeConfiguration.OpenOrders,
                        OrderDeposit = TradeConfiguration.OrderDeposit,
                        OrderIndent = TradeConfiguration.OrderIndent,
                        OrderStepPlus = TradeConfiguration.OrderStepPlus,
                        Profit = TradeConfiguration.Profit,
                        SqueezeProfit = TradeConfiguration.SqueezeProfit,
                        Strategy = TradeConfiguration.Strategy,
                        TrallingForvard = TradeConfiguration.TrallingForvard,
                        TrallingStop = TradeConfiguration.TrallingStop
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
