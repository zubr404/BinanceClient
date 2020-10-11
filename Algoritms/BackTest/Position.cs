using Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms.BackTest
{
    /*
     * Например пара BTC/USDT
     */

    /// <summary>
    /// Открытая позиция
    /// </summary>
    public class Position
    {
        public long ID { get; private set; }
        public string Pair { get; private set; } // "BTC"
        public double Amount { get; private set; } // в BTC
        public double Cost { get; private set; } // куплено/продано на сумму в USDT
        public double Price { get; private set; } //  (она же средняя цена)
        public double Profit { get; set; }
        public bool IsLong { get; private set; }
        public bool IsClose { get; private set; }

        public Position(string pair, double amount, double price, bool isLong)
        {
            ID = DateTime.Now.ToUnixTime();
            Pair = pair;
            IsLong = isLong;
            IcreasePosition(amount, price);
        }

        /// <summary>
        /// Увеличить позицию
        /// </summary>
        public void IcreasePosition(double amount, double price)
        {
            Amount += amount;
            Cost += amount * price;
            Price = Cost / Amount;
        }

        /// <summary>
        /// Закрыть позицию
        /// </summary>
        /// <param name="price"></param>
        public void ClosePosition(double price)
        {
            double finalCost = Amount * price;
            if (IsLong)
            {
                Profit = finalCost - Cost;
            }
            else
            {
                Profit = Cost - finalCost;
            }
            IsClose = true;
        }
    }
}
