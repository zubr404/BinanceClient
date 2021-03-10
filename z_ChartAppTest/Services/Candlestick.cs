using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    public class Candlestick
    {
        public double GridHeight { get; private set; }
        public double GridWidth { get; private set; }
        public ObservableCollection<CandleView> CandleViews { get; private set; }

        public Candlestick()
        {
            GridHeight = 300;
            GridWidth = 500;
            CandleViews = new ObservableCollection<CandleView>()
            {
                new CandleView()
                {
                    Date = 10,
                    TopPoint = 7,
                    HeightLine = 90,
                    HeightRect = 20,
                    WidthRect = 7,
                    TopPointRect = 5,
                    IsPositive = true
                },
                new CandleView()
                {
                    Date = 30,
                    TopPoint = 35,
                    HeightLine = 90,
                    HeightRect = 20,
                    WidthRect = 7,
                    TopPointRect = 17,
                    IsPositive = false
                }
            };
        }


    }
}
