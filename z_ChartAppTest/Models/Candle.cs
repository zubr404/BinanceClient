using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Models
{
    public class Candle
    {
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

        public Candle(double high, double low, double open, double close)
        {
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
