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
        /// Получаем сделки в диапазоне TadeID
        /// </summary>
        /// <param name="startTradeId"></param>
        /// <param name="stopTradeId"></param>
        /// <returns></returns>
        public IEnumerable<TradeHistory> Get(string pair, long startTradeId, long stopTradeId)
        {
            return db.TradeHistories.AsNoTracking().Where(x => x.Pair == pair && x.TradeId >= startTradeId && x.TradeId <= stopTradeId).OrderBy(x => x.TradeId);
        }

        /// <summary>
        /// Минимальный TadeID для стартового времени
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public long MinId(long startTime)
        {
            try
            {
                return db.TradeHistories.Where(x => x.Time >= startTime).Min(x => x.TradeId);
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Максимальный TadeID для финишного времени
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public long MaxId(long endTime)
        {
            try
            {
                return db.TradeHistories.Where(x => x.Time <= endTime).Max(x => x.TradeId);
            }
            catch
            {
                return 0;
            }
            
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
            catch(Exception ex)
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
