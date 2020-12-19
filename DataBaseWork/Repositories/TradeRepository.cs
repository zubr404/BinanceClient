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
        public bool Exists(Trade item)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.Any(x => x.FK_PublicKey == item.FK_PublicKey && x.TradeID == item.TradeID);
            }
        }

        public IEnumerable<Trade> Get(string publicKey, long unixTime, bool isBuyer)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.AsNoTracking().Where(x => x.FK_PublicKey == publicKey && x.Time > unixTime && x.IsBuyer == isBuyer).ToArray();
            }
        }

        public IEnumerable<Trade> Get(string publicKey, string pair, long unixTime, bool isBuyer)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.AsNoTracking().Where(x => x.FK_PublicKey == publicKey && x.Symbol.ToLower() == pair.ToLower() && x.Time > unixTime && x.IsBuyer == isBuyer).ToArray();
            }
        }

        public List<Trade> Get(string simbol, double minPrice, double maxPrice)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.AsNoTracking().Where(x => x.Symbol == simbol && x.Price >= minPrice && x.Price <= maxPrice).ToList();
            }
        }

        public long GetTimeLastTrade(string publicKey, bool isBuyer, long unixTime)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.Where(x => x.FK_PublicKey == publicKey && x.IsBuyer == isBuyer && x.Time >= unixTime).OrderByDescending(x => x.Time).Select(x => x.Time).FirstOrDefault();
            }
        }

        public long GetTimeLastTrade(string publicKey, string pair, bool isBuyer, long unixTime)
        {
            using (var db = new DataBaseContext())
            {
                return db.Trades.Where(x => x.FK_PublicKey == publicKey && x.Symbol.ToLower() == pair.ToLower() && x.IsBuyer == isBuyer && x.Time >= unixTime).OrderByDescending(x => x.Time).Select(x => x.Time).FirstOrDefault();
            }
        }

        public long GetMaxId(string publicKey, string simbol)
        {
            long result = -1;
            try
            {
                using (var db = new DataBaseContext())
                {
                    result = db.Trades.AsNoTracking().Where(x => x.FK_PublicKey == publicKey && x.Symbol == simbol).Select(x => x.TradeID).Max();
                }
            }
            catch { }
            return result;
        }

        public Trade Create(Trade item)
        {
            try
            {
                if (!Exists(item))
                {
                    using (var db = new DataBaseContext())
                    {
                        var trade = db.Trades.Add(item);
                        db.SaveChanges();
                        return trade.Entity;
                    }
                }
                return null;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public void Create(IEnumerable<Trade> trades)
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    db.Trades.AddRange(trades);
                    db.SaveChanges();
                }
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }
    }
}
