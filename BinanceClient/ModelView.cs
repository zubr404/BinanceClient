using BinanceClient.Services;
using BinanceClient.ViewModel;
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

        public static ConsoleScrin1 ConsoleScrin1 { get; private set; }

        public ModelView()
        {
            ConsoleScrin1 = new ConsoleScrin1();
            dispatcher = Dispatcher.CurrentDispatcher;
            ChartService = new ChartService(dispatcher);
        }
    }
}
