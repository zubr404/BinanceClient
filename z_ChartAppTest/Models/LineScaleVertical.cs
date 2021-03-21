using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Models
{
    /// <summary>
    /// Диния вертикальной сетки
    /// </summary>
    public class LineScaleVertical
    {
        /// <summary>
        /// Координата для линии от левого края
        /// </summary>
        public double LeftPointLine { get; set; }
        /// <summary>
        /// Длина линии сетки
        /// </summary>
        public double HeighLine { get; set; }
        /// <summary>
        /// Координата для надписи от верхнего края
        /// </summary>
        public double TopPointLabel { get; set; }
        /// <summary>
        /// Надпись
        /// </summary>
        public string TimeLabel { get; set; }
    }
}
