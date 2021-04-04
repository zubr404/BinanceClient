using System;
using System.Collections.Generic;
using System.Text;
using Charts.Interfaces;

namespace Charts.Models
{
    public class CandleView : IElementChartView
    {
        /// <summary>
        /// Координата времени - ось X
        /// </summary>
        public double LeftPoint { get; set; }
        /// <summary>
        /// Координата верхней точки свечи - ось Y
        /// </summary>
        public double TopPoint { get; set; }


        /// <summary>
        /// Координата верхней точки тела свечи - ось Y
        /// </summary>
        public double TopPointRect { get; set; }
        /// <summary>
        /// Высота тела свечи
        /// </summary>
        public double HeightRect { get; set; }
        /// <summary>
        /// Ширина тела свечи
        /// </summary>
        public double Width
        {
            get { return widthRect; }
            set
            {
                widthRect = value;
                XLine = (widthRect - WitdhLine) / 2;
            }
        }
        private double widthRect;

        /// <summary>
        /// Высота хвоста свечи
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// Ширина хвоста свечи
        /// </summary>
        public double WitdhLine { get; } = 1;
        /// <summary>
        /// Координата по оси Х для хвоста свечи
        /// </summary>
        public double XLine { get; private set; }

        /// <summary>
        /// Черная/белая
        /// </summary>
        public bool IsPositive { get; set; }

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

        public CandleView(long timeOpen, double high, double low, double open, double close)
        {
            this.timeOpen = timeOpen;
            this.high = high;
            this.low = low;
            this.open = open;
            this.close = close;
        }
    }
}
