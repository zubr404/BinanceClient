using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Models
{
    public class CandleView
    {
        /// <summary>
        /// Координата времени - ось X
        /// </summary>
        public double Date { get; set; }
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
        public double WidthRect
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
        public double HeightLine { get; set; }
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
    }
}
