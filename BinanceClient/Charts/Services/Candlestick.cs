using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Charts.Interfaces;
using Charts.Models;

namespace Charts.Services
{
    /// <summary>
    /// График свечей
    /// </summary>
    public class Candlestick : IChart
    {
        private const double WIDTH_RECT = 7;
        public ObservableCollection<IElementChartView> ElementChartViews { get; set; }
        private IEnumerable<Candle> candles;

        public Candlestick()
        {
            ElementChartViews = new ObservableCollection<IElementChartView>();
            candles = new List<Candle>();
        }

        public void SetCandles(IEnumerable<Candle> candles)
        {
            if (candles?.Count() > 0)
            {
                this.candles = candles.ToList();
            }
        }

        public void Create(double maxAllChart, double scaleIntervalPrice)
        {
            if (candles.Count() > 0)
            {
                ElementChartViews.Clear();
                var candleList = candles.ToList();
                var previosDate = 20;
                foreach (var candle in candleList)
                {
                    var date = previosDate;
                    previosDate += 10;

                    ElementChartViews.Add(new CandleView(candle.TimeOpen, candle.High, candle.Low, candle.Open, candle.Close)
                    {
                        LeftPoint = date,
                        TopPoint = GetTopPoint(candle.High, maxAllChart, scaleIntervalPrice),
                        Height = GetHeightLine(candle.High, candle.Low, scaleIntervalPrice),
                        HeightRect = GetHeightRect(candle.Open, candle.Close, scaleIntervalPrice),
                        Width = WIDTH_RECT,
                        TopPointRect = GetTopPointRect(candle.High, candle.Open, candle.Close, candle.IsPositive, scaleIntervalPrice),
                        IsPositive = candle.IsPositive
                    });
                }
            }
        }

        private double GetTopPoint(double highCandele, double maxAllChart, double scaleIntervalPrice)
        {
            return (maxAllChart - highCandele) * scaleIntervalPrice;
        }

        private double GetHeightLine(double highCandle, double lowCandle, double scaleIntervalPrice)
        {
            return (highCandle - lowCandle) * scaleIntervalPrice;
        }

        private double GetHeightRect(double openCandle, double closeCandle, double scaleIntervalPrice)
        {
            return Math.Abs(openCandle - closeCandle) * scaleIntervalPrice;
        }

        private double GetTopPointRect(double highCandle, double openCandle, double closeCandle, bool isPositive, double scaleIntervalPrice)
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
