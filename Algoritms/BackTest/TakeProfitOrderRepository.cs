using DataBaseWork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.BackTest
{
    public class TakeProfitOrderRepository
    {
        readonly List<TakeProfitOrderTest> takeProfitOrders;
        public TakeProfitOrderRepository()
        {
            takeProfitOrders = new List<TakeProfitOrderTest>();
        }
        public IEnumerable<TakeProfitOrderTest> GetActive(string pair) // по всем счетам
        {
            return takeProfitOrders.Where(x => x.Active && x.Pair.ToLower() == pair.ToLower());
        }

        public IEnumerable<TakeProfitOrderTest> GetActive(string publicKey, string pair)
        {
            return takeProfitOrders.Where(x => x.Active && x.FK_PublicKey == publicKey && x.Pair.ToLower() == pair.ToLower());
        }

        public void DeactivationAllOrders() // снятие по всем счетам
        {
            for (int i = 0; i < takeProfitOrders.Count; i++)
            {
                takeProfitOrders[i].Active = false;
            }
        }

        public void DeactivationAllOrders(string publicKey) // снятие по счетy
        {
            for (int i = 0; i < takeProfitOrders.Count; i++)
            {
                var order = takeProfitOrders[i];
                if (order.FK_PublicKey == publicKey)
                {
                    order.Active = false;
                }
            }
            ClearOrder();
        }

        public void Create(TakeProfitOrderTest item)
        {
            takeProfitOrders.Add(item);
        }

        public void UpdateExtremumPrice(Guid id, double price)
        {
            var order = takeProfitOrders.FirstOrDefault(x => x.ID == id);
            if (order != null)
            {
                order.ExtremumPrice = price;
            }
        }

        private void ClearOrder()
        {
            takeProfitOrders.RemoveAll(x => x.Active == false);
        }
    }
}
