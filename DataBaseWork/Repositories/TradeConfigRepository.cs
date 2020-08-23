using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TradeConfigRepository
    {
        readonly DataBaseContext db;
        public TradeConfigRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public TradeConfiguration GetLast()
        {
            var config = db.TradeConfigurations.OrderByDescending(x => x.ID).FirstOrDefault();
            return config;
        }

        public TradeConfiguration Update(TradeConfiguration configuration)
        {
            var config = db.TradeConfigurations.FirstOrDefault(x => x.AltCoin == configuration.AltCoin && x.MainCoin == configuration.MainCoin);
            if(config == null)
            {
                configuration.ActivationTime = GetTimeString();
                configuration.DeactivationTime = "";
                config = db.TradeConfigurations.Add(configuration).Entity;
                Save();
                DeactivationConfig(config.ID);
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
                config.OrderDeposit = configuration.OrderDeposit;
                config.FirstStep = configuration.FirstStep;
                config.OrderStepPlus = configuration.OrderStepPlus;
                config.Martingale = configuration.Martingale;
                config.DepositLimit = configuration.DepositLimit;
                config.TrallingStop = configuration.TrallingStop;
                config.Profit = configuration.Profit;
                config.TrallingForvard = configuration.TrallingForvard;
                config.SqueezeProfit = configuration.SqueezeProfit;
                config.Active = configuration.Active;
                config.ActivationTime = GetTimeString();
                config.DeactivationTime = "";
                Save();
                DeactivationConfig(config.ID);
            }
            return config;
        }

        private void DeactivationConfig(int id)
        {
            var configs = db.TradeConfigurations.Where(x => x.ID != id).AsEnumerable().Select(c => 
            { 
                c.Active = false;
                if (string.IsNullOrWhiteSpace(c.DeactivationTime))
                {
                    c.DeactivationTime = GetTimeString();
                }
                return c; 
            });
            foreach (var config in configs)
            {
                db.Entry(config).State = EntityState.Modified;
            }
            Save();
        }

        private string GetTimeString()
        {
            var now = DateTime.Now;
            return $"{now.Year}-{now.Month.ToString("00")}-{now.Day.ToString("00")} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}";
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
