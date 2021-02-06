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
        /// <summary>
        /// Проверяет наличие сделки по биржевому ID
        /// </summary>
        /// <param name="marketTradeId">Биржевой ID сделки</param>
        /// <returns></returns>
        public bool Exists(string pair, long marketTradeId)
        {
            using (var db = new DataBaseContext())
            {
                return db.TradeHistories.AsNoTracking().Any(x => x.TradeId == marketTradeId && x.Pair == pair);
            }
        }

        /// <summary>
        /// Получаем все записи по паре
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TradeHistory> Get(string pair)
        {
            using (var db = new DataBaseContext())
            {
                return db.TradeHistories.AsNoTracking().Where(x => x.Pair == pair).ToArray();
            }
        }

        /// <summary>
        /// Получаем сделки в диапазоне TadeID
        /// </summary>
        /// <param name="startTradeId"></param>
        /// <param name="stopTradeId"></param>
        /// <returns></returns>
        public IEnumerable<TradeHistory> Get(string pair, long startTradeId, long stopTradeId)
        {
            using (var db = new DataBaseContext())
            {
                return db.TradeHistories.AsNoTracking().Where(x => x.Pair == pair && x.TradeId >= startTradeId && x.TradeId <= stopTradeId).OrderBy(x => x.TradeId).ToArray();
            }
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
                using (var db = new DataBaseContext())
                {
                    return db.TradeHistories.Where(x => x.Time >= startTime).Min(x => x.TradeId);
                }
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
                using (var db = new DataBaseContext())
                {
                    return db.TradeHistories.Where(x => x.Time <= endTime).Max(x => x.TradeId);
                }
            }
            catch
            {
                return 0;
            }
            
        }

        /// <summary>
        /// Вставка с проверкой по Id
        /// </summary>
        /// <param name="trades"></param>
        public void AddRange(IEnumerable<TradeHistory> trades)
        {
            if(trades?.Count() > 0) 
            {
                try
                {
                    using (var db = new DataBaseContext())
                    {
                        var minId = trades.Min(x => x.TradeId);
                        var maxId = trades.Max(x => x.TradeId);
                        var tradesIdFromDb = db.TradeHistories.AsNoTracking().Where(x => x.TradeId >= minId && x.TradeId <= maxId)?.Select(x => x.TradeId).ToList();

                        if (tradesIdFromDb?.Count() > 0)
                        {
                            var tradesIdNew = trades.Select(x => x.TradeId).ToList();
                            var exeptId = tradesIdNew.Except(tradesIdFromDb).ToList();
                            if(exeptId?.Count() > 0)
                            {
                                var tradesForLoadDb = trades.Where(x => exeptId.Contains(x.TradeId));
                                if (tradesForLoadDb?.Count() > 0)
                                {
                                    db.TradeHistories.AddRange(tradesForLoadDb);
                                    db.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            db.TradeHistories.AddRange(trades);
                            db.SaveChanges();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
