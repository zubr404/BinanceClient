using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Models
{
    /// <summary>
    /// Линия горизонтальной сетки
    /// </summary>
    public class LineScaleHorizontal
    {
        /// <summary>
        /// Координата для линии от верхнего края
        /// </summary>
        public double TopPointLine { get; set; }
        /// <summary>
        /// Длина линии сетки
        /// </summary>
        public double WidthLine { get; set; }
        /// <summary>
        /// Координата для надписи от верхнего края
        /// </summary>
        public double TopPointLabel { get; set; }
        /// <summary>
        /// Надпись
        /// </summary>
        public string PriceLabel { get; set; }
    }
}
