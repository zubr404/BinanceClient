using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    /// <summary>
    /// Горизонтальная сетка
    /// </summary>
    public class ScaleHorizontal
    {
        private const double COUNT_DIVISION = 5;
        public ObservableCollection<LineScaleHorizontal> LineScaleHorizontals { get; private set; }

        public ScaleHorizontal()
        {
            LineScaleHorizontals = new ObservableCollection<LineScaleHorizontal>();
        }
        public void ScaleBuild(double heightPanel, double widhPanel, double maxAllChart, double deltaAllChart, int digits)
        {
            LineScaleHorizontals.Clear();
            var deltaLine = heightPanel / COUNT_DIVISION;
            var deltaPrice = deltaAllChart / COUNT_DIVISION;
            var pi = 0d;
            for (double i = 0; i <= heightPanel; i+= deltaLine)
            {
                LineScaleHorizontals.Add(new LineScaleHorizontal()
                {
                    TopPointLine = i,
                    WidthLine = widhPanel,
                    TopPointLabel = i,
                    PriceLabel = Math.Round(maxAllChart - pi, digits).ToString()
                });
                pi += deltaPrice;
            }
        }

        private void CreateLines()
        {

        }
    }
}
