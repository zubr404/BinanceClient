using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

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
        /// <summary>
        /// Цвет линии
        /// </summary>
        public SolidColorBrush ColorLine { get; set; }
        /// <summary>
        /// Цвет фона надписи
        /// </summary>
        public SolidColorBrush BackgroundLabel { get; set; }
        /// <summary>
        /// размер шрифта надписи
        /// </summary>
        public double FontSize { get; set; }
        /// <summary>
        /// Цвет рвмки надписи
        /// </summary>
        public SolidColorBrush BorderBrush { get; set; }
        /// <summary>
        /// Пунктир
        /// </summary>
        public DoubleCollection StrokeDashArray { get; set; }
        public Thickness Padding { get; set; } = new Thickness(15, 0, 0, 0);
    }
}
