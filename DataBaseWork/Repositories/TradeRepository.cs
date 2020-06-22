using DataBaseWork.Models;
using System;
using System.Collections.Generic;
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

        public void Create(IEnumerable<Trade> items)
        {
            try
            {
                db.Trades.AddRange(items);
                Save();
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public Trade Create(Trade item)
        {
            try
            {
                var trade = db.Trades.Add(item);
                Save();
                return trade.Entity;
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
