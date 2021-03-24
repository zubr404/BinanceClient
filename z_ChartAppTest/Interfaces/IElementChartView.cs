using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Interfaces
{
    /// <summary>
    /// Графический элемент основного графика: линии, точки, свечи и т.д.
    /// </summary>
    public interface IElementChartView
    {
        /// <summary>
        /// Координата времени - ось X
        /// </summary>
        public double LeftPoint { get; set; }
        /// <summary>
        /// Координата верхней левой точки - ось Y
        /// </summary>
        public double TopPoint { get; set; }
        /// <summary>
        /// Ширина элемента
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Высота элемента
        /// </summary>
        public double Height{ get; set; }
        /// <summary>
        /// Время открытия
        /// </summary>
        public long TimeOpen { get; }
    }
}
