using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DataBaseWork.Repositories
{
    public class StopLimitOrderRepository
    {
        readonly DataBaseContext db;
        public StopLimitOrderRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool ExistsActive() // по всем счетам
        {
            return db.StopLimitOrders.Any(x => x.Active);
        }

        public double GetMaxStopPriceBuy(string pair) // по всем счетам
        {
            var orders = db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && x.IsBuyOperation).AsNoTracking();
            if(orders != null)
            {
                if(orders.Count() > 0)
                {
                    return orders.Max(x => x.StopPrice);
                }
            }
            return 0;
        }

        public double GetMinStopPriceSell(string pair) // по всем счетам
        {
            var orders = db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && !x.IsBuyOperation).AsNoTracking();
            if (orders != null)
            {
                if (orders.Count() > 0)
                {
                    return orders.Min(x => x.StopPrice);
                }
            }
            return 0;
        }

        public IEnumerable<StopLimitOrder> GetActive(string pair)
        {
            return db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsNoTracking();
        }

        public IEnumerable<StopLimitOrder> GetActive(string publicKey, string pair)
        {
            return db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsNoTracking();
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            var orders = db.StopLimitOrders.Where(x => x.Active).AsEnumerable().Select(o =>
            {
                o.Active = false;
                return o;
            });
            foreach (var order in orders)
            {
                db.Entry(order).State = EntityState.Modified;
            }
            Save();
        }

        public void DeactivationAllOrders(string publicKey, bool isBuyer) // снятие по счетy and operation
        {
            var orders = db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.IsBuyOperation == isBuyer).AsEnumerable().Select(o =>
            {
                o.Active = false;
                return o;
            });
            foreach (var order in orders)
            {
                db.Entry(order).State = EntityState.Modified;
            }
            Save();
        }

        public void DeactivationAllOrders(string publicKey) // снятие по счетy
        {
            var orders = db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey).AsEnumerable().Select(o =>
            {
                o.Active = false;
                return o;
            });
            foreach (var order in orders)
            {
                db.Entry(order).State = EntityState.Modified;
            }
            Save();
        }

        public void DeactivateOrder(int id)
        {
            var order = db.StopLimitOrders.FirstOrDefault(x => x.ID == id);
            if(order != null)
            {
                order.Active = false;
                Save();
            }
        }

        public StopLimitOrder Create(StopLimitOrder item)
        {
            var order = db.StopLimitOrders.Add(item).Entity;
            Save();
            return order;
        }

        public void Create(IEnumerable<StopLimitOrder> items)
        {
            db.StopLimitOrders.AddRange(items);
            Save();
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
