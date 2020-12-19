using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TradeConfigRepository
    {
        public TradeConfiguration GetLast()
        {
            using(var db = new DataBaseContext())
            {
                var config = db.TradeConfigurations.Where(x => x.Active).OrderByDescending(x => x.ID).FirstOrDefault();
                return config;
            }
        }

        public IEnumerable<TradeConfiguration> GetActive()
        {
            using (var db = new DataBaseContext())
            {
                var configs = db.TradeConfigurations.Where(x => x.Active).ToArray();
                return configs;
            }
        }

        public TradeConfiguration Get(string baseAsset, string quoteAsset, string strategy)
        {
            using (var db = new DataBaseContext())
            {
                var config = db.TradeConfigurations.OrderByDescending(x => x.ActivationTime).FirstOrDefault(x => x.MainCoin.ToUpper() == baseAsset.ToUpper() && x.AltCoin.ToUpper() == quoteAsset.ToUpper() && x.Strategy.ToUpper() == strategy.ToUpper());
                return config;
            }
        }

        public TradeConfiguration Update(TradeConfiguration configuration, string savedStrategies)
        {
            if (string.IsNullOrWhiteSpace(configuration.AltCoin) || string.IsNullOrWhiteSpace(configuration.MainCoin))
            {
                throw new ArgumentException("Поля AltCoin и MainCoin не должны содержать пустых значений.", "configuration");
            }
            else
            {
                var strategyString = GetStrategyString(configuration);
                using (var db = new DataBaseContext())
                {
                    var config = db.TradeConfigurations.FirstOrDefault(x => x.AltCoin == configuration.AltCoin && x.MainCoin == configuration.MainCoin);
                    if (config == null)
                    {
                        configuration.ActivationTime = DateTime.UtcNow.ToUnixTime();
                        configuration.DeactivationTime = 0;
                        config = db.TradeConfigurations.Add(configuration).Entity;
                        db.SaveChanges();

                        if (!savedStrategies.Contains(strategyString))
                        {
                            DeactivationConfig(config.ID);
                        }
                    }
                    else
                    {
                        config.MainCoin = configuration.MainCoin;
                        config.AltCoin = configuration.AltCoin;
                        config.Strategy = configuration.Strategy;
                        config.IntervalHttp = configuration.IntervalHttp;
                        config.Margin = configuration.Margin;
                        config.OpenOrders = configuration.OpenOrders;
                        config.OrderIndent = configuration.OrderIndent;
                        config.OrderReload = configuration.OrderReload;
                        config.OrderDeposit = configuration.OrderDeposit;
                        config.FirstStep = configuration.FirstStep;
                        config.OrderStepPlus = configuration.OrderStepPlus;
                        config.Martingale = configuration.Martingale;
                        config.DepositLimit = configuration.DepositLimit;
                        config.Loss = configuration.Loss;
                        config.Profit = configuration.Profit;
                        config.IndentExtremum = configuration.IndentExtremum;
                        config.ProtectiveSpread = configuration.ProtectiveSpread;
                        config.Active = configuration.Active;
                        config.ActivationTime = DateTime.UtcNow.ToUnixTime();
                        config.DeactivationTime = 0;
                        db.SaveChanges();

                        if (!savedStrategies.Contains(strategyString))
                        {
                            DeactivationConfig(config.ID);
                        }
                    }
                    return config;
                }
            }
        }

        private string GetStrategyString(TradeConfiguration tradeConfiguration)
        {
            return $"{tradeConfiguration.MainCoin.ToUpper()}{tradeConfiguration.AltCoin.ToUpper()}{tradeConfiguration.Strategy.ToUpper()}";
        }

        private void DeactivationConfig(int id)
        {
            using (var db = new DataBaseContext())
            {
                var configs = db.TradeConfigurations.Where(x => x.ID != id).AsEnumerable().Select(c =>
                {
                    c.Active = false;
                    if (c.DeactivationTime == 0)
                    {
                        c.DeactivationTime = DateTime.UtcNow.ToUnixTime();
                    }
                    return c;
                });
                foreach (var config in configs)
                {
                    db.Entry(config).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }
    }
}
