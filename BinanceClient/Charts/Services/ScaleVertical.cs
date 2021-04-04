using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Charts.Interfaces;
using Charts.Models;
using Services;

namespace Charts.Services
{
    public class ScaleVertical
    {
        private const double COUNT_DIVISION = 5;
        public ObservableCollection<LineScaleVertical> LineScaleVerticals { get; private set; }

        public ScaleVertical()
        {
            LineScaleVerticals = new ObservableCollection<LineScaleVertical>();
        }

        public void ScaleBuild(IEnumerable<IElementChartView> candles, double heightPanel, double widhPanel)
        {
            if(candles?.Count() > 0)
            {
                LineScaleVerticals.Clear();
                int lenCandles = candles.Count();
                int intervalCandles = (int)(lenCandles / COUNT_DIVISION);
                for (int i = 0; i <= lenCandles; i += intervalCandles)
                {
                    if (i == intervalCandles)
                    {
                        i--;
                    }
                    var candle = candles.ElementAt(i);
                    var timeOpen = candle.TimeOpen.UnixToDateTime();
                    LineScaleVerticals.Add(new LineScaleVertical()
                    {
                        LeftPointLine = candle.LeftPoint + candle.Width / 2,
                        HeighLine = heightPanel,
                        TopPointLabel = heightPanel,
                        TimeLabel = $"{timeOpen.Date:dd.MM.yyyy}\n{timeOpen.TimeOfDay}"
                    });
                }
            }
        }
    }
}
