using System;
using System.Collections.Generic;
using System.Text;
using z_ChartAppTest.Interfaces;

namespace z_ChartAppTest.Models
{
    public class Candle : IElementChart
    {
        public long TimeOpen { get { return timeOpen; } }
        private long timeOpen;
        public double High { get { return high; } }
        private double high;

        public double Low { get { return low; } }
        private double low;

        public double Open { get { return open; } }
        private double open;

        public double Close { get { return close; } }
        private double close;

        public bool IsPositive { get { return ispositive; } }
        private bool ispositive;

        public Candle(long timeOpen, double high, double low, double open, double close)
        {
            this.timeOpen = timeOpen;
            this.high = high;
            this.low = low;
            this.open = open;
            this.close = close;
            SetIsPositive();
        }

        private void SetIsPositive()
        {
            if(Close > Open)
            {
                ispositive = true;
            }
            else
            {
                ispositive = false;
            }
        }
    }
}
