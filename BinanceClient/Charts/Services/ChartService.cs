using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Charts.Interfaces;
using Charts.Models;
using DataBaseWork.Repositories;

namespace Charts.Services
{
    public class ChartService
    {
        public IChart Chart { get; private set; }
        public ScaleHorizontal ScaleHorizontal { get; private set; }
        public ScaleVertical ScaleVertical { get; private set; }
        public HorizontalLinePrice HorizontalLinePrice { get; private set; }
        public AdditionalHorizontalLine AdditionalHorizontalLine { get; private set; }

        public ChartService(IChart chart)
        {
            Chart = chart;
            ScaleHorizontal = new ScaleHorizontal();
            ScaleVertical = new ScaleVertical();
            HorizontalLinePrice = new HorizontalLinePrice();
            AdditionalHorizontalLine = new AdditionalHorizontalLine();
        }

        public void ChartBuild(IEnumerable<IElementChart> candles, string pair, double heightPanel, double widthPanel, double currentPrice, int digits)
        {
            GetMaxAllChart(candles);
            GetMinAllChart(candles);
            GetDeltaAllChart();
            GetScaleIntervalPrice(heightPanel);

            // строим горизонтальную сетку
            ScaleHorizontal.ScaleBuild(heightPanel, widthPanel, MaxAllChart, DeltaAllChart, digits);

            // строим основной график
            Chart.Create(MaxAllChart, ScaleIntervalPrice);

            // строим вертикальную сетку
            ScaleVertical.ScaleBuild(Chart.ElementChartViews, heightPanel, widthPanel);

            // отображение текущей цены
            HorizontalLinePrice.LineCurrentPriceBuild(widthPanel, MaxAllChart, currentPrice, ScaleIntervalPrice);

            var trades = GetTrades(pair, MinAllChart, MaxAllChart);
            AdditionalHorizontalLine.AdditionalLinesBuild(trades, widthPanel, MaxAllChart, ScaleIntervalPrice);
            //------
        }

        private List<BuySellView> GetTrades(string simbol, double minPrice, double maxPrice)
        {
            var result = new List<BuySellView>();
            var tradeRepository = new TradeRepository();
            var trades = tradeRepository.Get(simbol, minPrice, maxPrice).ToList();
            var resGroup = trades.GroupBy(x => new { x.Price, x.IsBuyer }).ToList();
            foreach (var item in resGroup)
            {
                result.Add(new BuySellView()
                {
                    Price = item.Key.Price,
                    IsBuy = item.Key.IsBuyer
                });
            }
            return result;
        }

        public double MaxAllChart { get { return maxAllChart; } }
        private double maxAllChart;

        public double MinAllChart { get { return minAllChart; } }
        private double minAllChart;

        public double DeltaAllChart { get { return deltaAllChart; } }
        private double deltaAllChart;

        public double ScaleIntervalPrice { get { return scaleIntervalPrice; } }
        private double scaleIntervalPrice;

        /// <summary>
        /// Получить макс всего графика
        /// </summary>
        /// <param name="candles"></param>
        /// <returns></returns>
        private double GetMaxAllChart(IEnumerable<IElementChart> candles)
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
        private double GetMinAllChart(IEnumerable<IElementChart> candles)
        {
            if (candles?.Count() > 0)
            {
                minAllChart = candles.Min(x => x.Low);
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
        private double GetDeltaAllChart()
        {
            deltaAllChart = maxAllChart - minAllChart;
            return deltaAllChart;
        }

        /// <summary>
        /// Получаем цену деления для шкалы цены Y
        /// </summary>
        /// <returns></returns>
        private double GetScaleIntervalPrice(double heightPanel)
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
    }
}
