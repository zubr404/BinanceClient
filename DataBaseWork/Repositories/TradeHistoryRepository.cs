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

        /// <summary>
        /// Проверяет наличие сделки по биржевому ID
        /// </summary>
        /// <param name="marketTradeId">Биржевой ID сделки</param>
        /// <returns></returns>
        public bool Exists(string pair, long marketTradeId)
        {
            return db.TradeHistories.AsNoTracking().Any(x => x.TradeId == marketTradeId && x.Pair == pair);
        }

        /// <summary>
        /// Получаем все записи по паре
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TradeHistory> Get(string pair)
        {
            return db.TradeHistories.AsNoTracking().Where(x => x.Pair == pair);
        }

        /// <summary>
        /// Получаем сделки в диапазоне ID
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public IEnumerable<TradeHistory> Get(string pair, int startId, int stopId)
        {
            return db.TradeHistories.AsNoTracking().Where(x => x.Pair == pair && x.TradeId >= startId && x.TradeId <= stopId);
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
