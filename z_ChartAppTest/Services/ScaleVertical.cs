using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    public class ScaleVertical
    {
        private const double COUNT_DIVISION = 5;
        public ObservableCollection<LineScaleVertical> LineScaleVerticals { get; private set; }

        public ScaleVertical()
        {
            LineScaleVerticals = new ObservableCollection<LineScaleVertical>();
        }

        public void ScaleBuild(IEnumerable<Candle> candles, double heightPanel, double widhPanel)
        {
            if(candles?.Count() > 0)
            {
                int lenCandles = candles.Count();
                int intervalCandles = (int)(lenCandles / COUNT_DIVISION);
                for (int i = 0; i <= lenCandles; i += intervalCandles)
                {
                    LineScaleVerticals.Add(new LineScaleVertical() 
                    {
                    
                    });
                }
            }
        }
    }
}
