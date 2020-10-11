using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel.ScrinCalculator
{
    /// <summary>
    /// Данные по текущей сетке
    /// </summary>
    public class OrderDataView
    {
        /// <summary>
        /// Количество в валюте актива
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Количество в валюте котировки
        /// </summary>
        public double Equivalent { get; set; }
        /// <summary>
        /// Цена
        /// </summary>
        public double PriceInGrid { get; set; }
        /// <summary>
        /// Цена протита
        /// </summary>
        public double ProfitPrice { get; set; }
        /// <summary>
        /// Прибыль
        /// </summary>
        public double Rebount { get; set; }
    }
}
