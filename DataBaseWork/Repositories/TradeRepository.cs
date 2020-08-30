using DataBaseWork.Models;
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
