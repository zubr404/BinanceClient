using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TakeProfitOrderRepository
    {
        readonly DataBaseContext db;
        public TakeProfitOrderRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool ExistsActive() // по всем счетам
        {
            return db.TakeProfitOrders.Any(x => x.Active);
        }
        public IEnumerable<TakeProfitOrder> GetActive(string pair) // по всем счетам
        {
            return db.TakeProfitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsNoTracking();
        }

        public IEnumerable<TakeProfitOrder> GetActive(string publicKey, string pair)
        {
            return db.TakeProfitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsNoTracking();
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            var orders = db.TakeProfitOrders.Where(x => x.Active).AsEnumerable().Select(o =>
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
            var orders = db.TakeProfitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey).AsEnumerable().Select(o =>
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

        public TakeProfitOrder Create(TakeProfitOrder item)
        {
            var order = db.TakeProfitOrders.Add(item).Entity;
            Save();
            return order;
        }

        public void UpdateExtremumPrice(int id, double price)
        {
            var order = db.TakeProfitOrders.FirstOrDefault(x => x.ID == id);
            if (order != null)
            {
                order.ExtremumPrice = price;
                Save();
            }
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
