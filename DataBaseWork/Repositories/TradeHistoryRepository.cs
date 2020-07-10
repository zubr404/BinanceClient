using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TradeHistoryRepository
    {
        readonly DataBaseContext db;
        public TradeHistoryRepository(DataBaseContext db) 
        {
            this.db = db;
        }

        public IEnumerable<TradeHistory> Get(int startId, int stopId)
        {
            return db.TradeHistories.AsNoTracking().Where(x => x.TradeId >= startId && x.TradeId <= stopId);
        }

        public void AddRange(IEnumerable<TradeHistory> trades)
        {
            try
            {
                db.TradeHistories.AddRange(trades);
                Save();
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
