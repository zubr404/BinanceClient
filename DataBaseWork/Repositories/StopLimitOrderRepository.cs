﻿using DataBaseWork.Models;
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
        public bool ExistsActive() // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Any(x => x.Active);
            }
        }

        public bool ExistsActive(string pair) // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Any(x => x.Active && x.Pair.ToLower() == pair.ToLower());
            }
        }

        public double GetMaxStopPriceBuy(string pair) // по всем счетам
        {
            using (var db = new DataBaseContext())
            {
                var orders = db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && x.IsBuyOperation).AsNoTracking();
                if (orders != null)
                {
                    if (orders.Count() > 0)
                    {
                        return orders.Max(x => x.StopPrice);
                    }
                }
                return 0;
            }
        }

        public double GetMinStopPriceSell(string pair) // по всем счетам
        {
            using (var db = new DataBaseContext())
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
        }

        public IEnumerable<StopLimitOrder> GetActive(string pair)
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsNoTracking().ToArray();
            }
        }

        public IEnumerable<StopLimitOrder> GetStopOrdersFilledBuy(string pair, double price)
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && price <= x.StopPrice).AsNoTracking().ToArray();
            }
        }

        public IEnumerable<StopLimitOrder> GetStopOrdersFilledSell(string pair, double price)
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && price >= x.StopPrice).AsNoTracking().ToArray();
            }
        }

        public IEnumerable<StopLimitOrder> GetActive(string publicKey, string pair)
        {
            using (var db = new DataBaseContext())
            {
                return db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsNoTracking().ToArray();
            }
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            using (var db = new DataBaseContext())
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
                db.SaveChanges();
            }
        }

        public void DeactivationAllOrders(string publicKey, bool isBuyer) // снятие по счетy and operation
        {
            using (var db = new DataBaseContext())
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
                db.SaveChanges();
            }
        }


        public void DeactivationAllOrders(string publicKey, string pair, bool isBuyer) // снятие по счетy and pair and operation
        {
            using (var db = new DataBaseContext())
            {
                var orders = db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower() && x.IsBuyOperation == isBuyer).AsEnumerable().Select(o =>
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
                var orders = db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey).AsEnumerable().Select(o =>
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
                var orders = db.StopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower()).AsEnumerable().Select(o =>
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
                var orders = db.StopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower()).AsEnumerable().Select(o =>
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

        public void DeactivateOrder(int id)
        {
            using (var db = new DataBaseContext())
            {
                var order = db.StopLimitOrders.FirstOrDefault(x => x.ID == id);
                if (order != null)
                {
                    order.Active = false;
                    db.SaveChanges();
                }
            }
        }

        public StopLimitOrder Create(StopLimitOrder item)
        {
            using (var db = new DataBaseContext())
            {
                var order = db.StopLimitOrders.Add(item).Entity;
                db.SaveChanges();
                return order;
            }
        }

        public void Create(IEnumerable<StopLimitOrder> items)
        {
            using (var db = new DataBaseContext())
            {
                db.StopLimitOrders.AddRange(items);
                db.SaveChanges();
            }
        }
    }
}
