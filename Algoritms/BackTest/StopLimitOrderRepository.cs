using DataBaseWork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.BackTest
{
    public class StopLimitOrderRepository
    {
        readonly List<StopLimitOrderTest> stopLimitOrders;
        public StopLimitOrderRepository()
        {
            stopLimitOrders = new List<StopLimitOrderTest>();
        }

        public double MaxStopPriceBuy { get; private set; }
        public double MinStopPriceSell { get; private set; }

        private double GetMaxStopPriceBuy(string pair) // по всем счетам
        {
            var orders = stopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && x.IsBuyOperation);
            if (orders != null)
            {
                if (orders.Count() > 0)
                {
                    return orders.Max(x => x.StopPrice);
                }
            }
            return 0;
        }

        private double GetMinStopPriceSell(string pair) // по всем счетам
        {
            var orders = stopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower() && !x.IsBuyOperation);
            if (orders != null)
            {
                if (orders.Count() > 0)
                {
                    return orders.Min(x => x.StopPrice);
                }
            }
            return 0;
        }

        public IEnumerable<StopLimitOrderTest> GetActive(string pair)
        {
            return stopLimitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower());
        }

        public IEnumerable<StopLimitOrderTest> GetActive(string publicKey, string pair)
        {
            return stopLimitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower());
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            for (int i = 0; i < stopLimitOrders.Count; i++)
            {
                stopLimitOrders[i].Active = false;
            }
        }

        public void DeactivationAllOrders(string publicKey, bool isBuyer) // снятие по счетy and operation
        {
            for (int i = 0; i < stopLimitOrders.Count; i++)
            {
                var order = stopLimitOrders[i];
                if (order.FK_PublicKey == publicKey && order.IsBuyOperation == isBuyer)
                {
                    order.Active = false;
                }
            }
            ClearOrders();
        }

        public void DeactivationAllOrders(string publicKey) // снятие по счетy
        {
            for (int i = 0; i < stopLimitOrders.Count; i++)
            {
                var order = stopLimitOrders[i];
                if (order.FK_PublicKey == publicKey)
                {
                    order.Active = false;
                }
            }
            ClearOrders();
        }

        public void DeactivateOrder(Guid id)
        {
            var order = stopLimitOrders.FirstOrDefault(x => x.ID == id);
            if (order != null)
            {
                order.Active = false;
            }
            ClearOrders();
        }

        public void Create(StopLimitOrderTest item)
        {
            stopLimitOrders.Add(item);
            MaxStopPriceBuy = GetMaxStopPriceBuy(item.Pair);
            MinStopPriceSell = GetMinStopPriceSell(item.Pair);
        }

        public void Create(IEnumerable<StopLimitOrderTest> items)
        {
            stopLimitOrders.AddRange(items);

            MaxStopPriceBuy = GetMaxStopPriceBuy(items.ElementAt(0).Pair);
            MinStopPriceSell = GetMinStopPriceSell(items.ElementAt(0).Pair);
        }

        private void ClearOrders()
        {
            stopLimitOrders.RemoveAll(x => x.Active == false);
        }
    }
}
