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
        public bool ExistsActive() // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                return db.TakeProfitOrders.Any(x => x.Active);
            }
        }

        public bool ExistsActive(string pair) // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                return db.TakeProfitOrders.Any(x => x.Active && x.Pair.ToLower() == pair.ToLower());
            }
        }

        public IEnumerable<TakeProfitOrder> GetActive(string pair) // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                return db.TakeProfitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsNoTracking().ToArray();
            }
        }

        public IEnumerable<TakeProfitOrder> GetActive(string publicKey, string pair)
        {
            using (var db = new DataBaseContext())
            {
                return db.TakeProfitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsNoTracking().ToArray();
            }
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            using (var db = new DataBaseContext())
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
                db.SaveChanges();
            }
        }

        public void DeactivationAllOrders(string publicKey) // снятие по счетy
        {
            using (var db = new DataBaseContext())
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
                db.SaveChanges();
            }
        }

        public void DeactivationAllOrders(string publicKey, string pair) // снятие по счетy и паре
        {
            using (var db = new DataBaseContext())
            {
                var orders = db.TakeProfitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsEnumerable().Select(o =>
                {
                    o.Active = false;
                    return o;
                });
                foreach (var order in orders)
                {
                    db.Entry(order).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        public void DeactivationForPair(string pair) // снятие по паре
        {
            using (var db = new DataBaseContext())
            {
                var orders = db.TakeProfitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsEnumerable().Select(o =>
                {
                    o.Active = false;
                    return o;
                });
                foreach (var order in orders)
                {
                    db.Entry(order).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        public TakeProfitOrder Create(TakeProfitOrder item)
        {
            using (var db = new DataBaseContext())
            {
                var order = db.TakeProfitOrders.Add(item).Entity;
                db.SaveChanges();
                return order;
            }
        }

        public void UpdateExtremumPrice(int id, double price)
        {
            using (var db = new DataBaseContext())
            {
                var order = db.TakeProfitOrders.FirstOrDefault(x => x.ID == id);
                if (order != null)
                {
                    order.ExtremumPrice = price;
                    db.SaveChanges();
                }
            }
        }
    }
}
