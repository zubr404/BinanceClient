using BinanceClient.Services;
using StockExchenge;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BinanceClient
{
    class ModelView
    {
        public ChartService ChartService { get; set; }

        public ModelView()
        {
            ChartService = new ChartService();
        }
    }
}
