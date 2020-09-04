using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TradeRepository
    {
        readonly DataBaseContext db;
        public TradeRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool Exists(Trade item)
        {
            return db.Trades.Any(x => x.FK_PublicKey == item.FK_PublicKey && x.TradeID == item.TradeID);
        }

        public IEnumerable<Trade> Get(string publicKey, long unixTime, bool isBuyer)
        {
            return db.Trades.AsNoTracking().Where(x => x.FK_PublicKey == publicKey && x.Time > unixTime && x.IsBuyer == isBuyer);
        }

        public long GetTimeLastTrade(string publicKey, bool isBuyer, long unixTime)
        {
            return db.Trades.Where(x => x.FK_PublicKey == publicKey && x.IsBuyer == isBuyer && x.Time >= unixTime).OrderByDescending(x => x.Time).Select(x => x.Time).FirstOrDefault();
        }

        public Trade Create(Trade item)
        {
            try
            {
                if (!Exists(item))
                {
                    var trade = db.Trades.Add(item);
                    Save();
                    return trade.Entity;
                }
                return null;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
