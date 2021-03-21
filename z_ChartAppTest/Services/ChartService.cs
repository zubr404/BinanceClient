using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using z_ChartAppTest.Interfaces;
using z_ChartAppTest.Models;

namespace z_ChartAppTest.Services
{
    public class ChartService
    {
        public IChart Chart { get; private set; }
        public ScaleHorizontal ScaleHorizontal { get; private set; }

        public ChartService(IChart chart)
        {
            Chart = chart;
            ScaleHorizontal = new ScaleHorizontal();
        }

        public void ChartBuild(IEnumerable<IElementChart> candles, double heightPanel, double widthPanel, int digits)
        {
            GetMaxAllChart(candles);
            GetMinAllChart(candles);
            GetDeltaAllChart();
            GetScaleIntervalPrice(heightPanel);

            // строим горизонтальную сетку
            ScaleHorizontal.ScaleBuild(heightPanel, widthPanel, MaxAllChart, DeltaAllChart, digits);

            // строим вертикальную сетку


            // строим основной график
            Chart.Create(MaxAllChart, ScaleIntervalPrice);
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
