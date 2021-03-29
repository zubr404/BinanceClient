using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    /// <summary>
    /// Дополнительные горизонтальные линии
    /// </summary>
    public class AdditionalHorizontalLine
    {
        public ObservableCollection<LineScaleHorizontal> AdditionalHorizontalLines { get; private set; }

        public AdditionalHorizontalLine()
        {
            AdditionalHorizontalLines = new ObservableCollection<LineScaleHorizontal>();
        }

        public void AdditionalLinesBuild(IEnumerable<BuySellView> buySellViews, double widhPanel, double maxAllChart, double scaleIntervalPrice)
        {
            if(buySellViews?.Count() > 0)
            {
                AdditionalHorizontalLines.Clear();
                foreach (var buysell in buySellViews)
                {
                    var topPoint = GetTopPoint(buysell.Price, maxAllChart, scaleIntervalPrice);
                    var color = Brushes.Red;
                    if (buysell.IsBuy)
                    {
                        color = Brushes.Green;
                    }

                    AdditionalHorizontalLines.Add(new LineScaleHorizontal()
                    {
                        TopPointLine = topPoint,
                        WidthLine = widhPanel - 80,
                        TopPointLabel = topPoint,
                        PriceLabel = buysell.Price.ToString(),
                        ColorLine = color,
                        StrokeDashArray = null,
                        Padding = new System.Windows.Thickness(3),
                        BackgroundLabel = Brushes.Black,
                        FontSize = 8,
                        BorderBrush = Brushes.White
                    });
                }
            }
        }

        private double GetTopPoint(double price, double maxAllChart, double scaleIntervalPrice)
        {
            return (maxAllChart - price) * scaleIntervalPrice;
        }
    }
}
