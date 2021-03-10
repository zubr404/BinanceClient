using System;
using System.Collections.Generic;
using System.Text;
using z_ChartAppTest.Services;

namespace z_ChartAppTest
{
    class ModelView
    {
        public Candlestick Candlestick { get; private set; }
        public ModelView()
        {
            Candlestick = new Candlestick();
        }
    }
}
