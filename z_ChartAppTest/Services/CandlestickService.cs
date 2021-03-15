using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    public class CandlestickService
    {
        public ObservableCollection<CandleView> CandleViews { get; private set; }

        public double MaxAllChart { get { return maxAllChart; } }
        private double maxAllChart;

        public double MinAllChart { get { return minAllChart; } }
        private double minAllChart;

        public double DeltaAllChart { get { return deltaAllChart; } }
        private double deltaAllChart;

        public double ScaleIntervalPrice { get { return scaleIntervalPrice; } }
        private double scaleIntervalPrice;

        public CandlestickService()
        {
            CandleViews = new ObservableCollection<CandleView>();
        }

        /// <summary>
        /// Получить макс всего графика
        /// </summary>
        /// <param name="candles"></param>
        /// <returns></returns>
        public double GetMaxAllChart(IEnumerable<Candle> candles)
        {
            if (candles?.Count() > 0)
            {
                maxAllChart = candles.Max(x => x.High);
                return maxAllChart;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Получить мин всего графика
        /// </summary>
        /// <param name="candles"></param>
        /// <returns></returns>
        public double GetMinAllChart(IEnumerable<Candle> candles)
        {
            if (candles?.Count() > 0)
            {
                minAllChart = candles.Min(x => x.High);
                return minAllChart;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Получить дельту всего графика
        /// </summary>
        /// <param name="candles"></param>
        /// <returns></returns>
        public double GetDeltaAllChart(IEnumerable<Candle> candles)
        {
            if(candles?.Count() > 0)
            {
                var maxPrice = candles.Max(x => x.High);
                var minPrice = candles.Min(x => x.Low);
                deltaAllChart = maxPrice - minPrice;
                return deltaAllChart;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Получаем цену деления для шкалы цены Y
        /// </summary>
        /// <returns></returns>
        public double GetScaleIntervalPrice(double heightPanel)
        {
            if (deltaAllChart > 0)
            {
                scaleIntervalPrice = heightPanel / deltaAllChart;
                return scaleIntervalPrice;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Создать график
        /// </summary>
        /// <param name="candles"></param>
        public void CreateChart(IEnumerable<Candle> candles)
        {
            if (candles?.Count() > 0)
            {
                CandleViews.Clear();
                var candleList = candles.ToList();
                var previosDate = 20;
                foreach (var candle in candleList)
                {
                    var date = previosDate;
                    previosDate += 10;

                    CandleViews.Add(new CandleView()
                    {
                        Date = date,
                        TopPoint = GetTopPoint(candle.High),
                        HeightLine = GetHeightLine(candle.High, candle.Low),
                        HeightRect = GetHeightRect(candle.Open, candle.Close),
                        WidthRect = 7,
                        TopPointRect = GetTopPointRect(candle.High, candle.Open, candle.Close, candle.IsPositive),
                        IsPositive = candle.IsPositive
                    });
                }
            }
        }

        private double GetTopPoint(double highCandele)
        {
            return (MaxAllChart - highCandele) * scaleIntervalPrice;
        }

        private double GetHeightLine(double highCandle, double lowCandle)
        {
            return (highCandle - lowCandle) * scaleIntervalPrice;
        }

        private double GetHeightRect(double openCandle, double closeCandle)
        {
            return Math.Abs(openCandle - closeCandle) * scaleIntervalPrice;
        }

        private double GetTopPointRect(double highCandle, double openCandle, double closeCandle, bool isPositive)
        {
            if (isPositive)
            {
                return (highCandle - closeCandle) * scaleIntervalPrice;
            }
            else
            {
                return (highCandle - openCandle) * scaleIntervalPrice;
            }
        }
    }
}
