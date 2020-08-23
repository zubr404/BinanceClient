using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BinanceClient.Models;
using System.Windows;

namespace BinanceClient.ViewModel.Scrin1
{
    public class CentralPanelScrin1
    {
        public TradeConfigurationView TradeConfiguration { get; private set; }
        readonly TradeConfigRepository configRepository;

        public CentralPanelScrin1(TradeConfigRepository configRepository)
        {
            TradeConfiguration = new TradeConfigurationView();
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
