using BinanceClient.Services;
using StockExchenge;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace BinanceClient
{
    class ModelView
    {
        public ChartService ChartService { get; set; }
        private readonly Dispatcher dispatcher;

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            ChartService = new ChartService(dispatcher);
        }
    }
}
