using Algoritms.Models;
using DataBaseWork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.MappingModel
{
    static class ModelsMapper
    {
        public static IEnumerable<TradeConfigurationAlgo> ToTradeConfigurationAlgos(this IEnumerable<TradeConfiguration> items)
        {
            if(items != null)
            {
                if (items.Any())
                {
                    var configs = new List<TradeConfigurationAlgo>();
                    foreach (var item in items)
                    {
                        configs.Add(item.ToTradeConfigurationAlgo());
                    }
                    return configs;
                }
            }
            return null;
        }
        public static TradeConfigurationAlgo ToTradeConfigurationAlgo(this TradeConfiguration item)
        {
            if(item != null)
            {
                return new TradeConfigurationAlgo()
                {
                    MainCoin = item.MainCoin,
                    AltCoin = item.AltCoin,
                    Strategy = item.Strategy,
                    Margin = item.Margin,
                    OpenOrders = item.OpenOrders,
                    OrderIndent = item.OrderIndent,
                    OrderDeposit = item.OrderDeposit,
                    FirstStep = item.FirstStep,
                    OrderStepPlus = item.OrderStepPlus,
                    Martingale = item.Martingale,
                    DepositLimit = item.DepositLimit,
                    OrderReload = item.OrderReload,
                    Loss = item.Loss,
                    Profit = item.Profit,
                    IndentExtremum = item.IndentExtremum,
                    ProtectiveSpread = item.ProtectiveSpread,
                    Active = item.Active,
                    ActivationTime = item.ActivationTime,
                    DeactivationTime = item.DeactivationTime
                };
            }
            return null;
        }
    }
}
