using System;
using System.Collections.Generic;
using System.Text;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    /// <summary>
    /// Отображение текущей цены на графике
    /// </summary>
    public class HorizontalLinePrice : PropertyChangedBase
    {
        public LineScaleHorizontal LineCurrentPrice
        {
            get { return lineCurrentPrice; }
            set
            {
                lineCurrentPrice = value;
                base.NotifyPropertyChanged();
            }
        }
        private LineScaleHorizontal lineCurrentPrice;

        public void LineCurrentPriceBuild(double widhPanel, double maxAllChart, double price, double scaleIntervalPrice)
        {
            var topPoint = GetTopPoint(price, maxAllChart, scaleIntervalPrice);
            LineCurrentPrice = new LineScaleHorizontal()
            {
                TopPointLine = topPoint,
                WidthLine = widhPanel,
                TopPointLabel = topPoint,
                PriceLabel = price.ToString()
            };
        }

        private double GetTopPoint(double price, double maxAllChart, double scaleIntervalPrice)
        {
            return (maxAllChart - price) * scaleIntervalPrice;
        }
    }
}
