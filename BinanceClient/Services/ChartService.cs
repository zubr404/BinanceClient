using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Text;
using StockExchenge;
using Services;
using System.Linq;
using System.Collections.ObjectModel;
using LiveCharts.Wpf;
using System.Drawing;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Threading;

namespace BinanceClient.Services
{
    class ChartService : PropertyChangedBase
    {
        public ReadOnlyCollection<string> Timeframes { get; set; }
        public ChartValues<OhlcPoint> OhclValues { get; set; }
        public List<string> LabelsX { get; set; }
        public Func<double, string> FormatterY { get; set; }
        public SeriesCollection Series { get; set; }
        public CandleSeries CandleSeries { get; set; }

        private Kline kline;
        private bool isClose = false;   // признак, что предыдущая свеча закрыта
        private string formatX = "";

        private Dispatcher dispatcher;

        #region Properties
        private string selectedInterval;
        public string SelectedInterval
        {
            get { return selectedInterval; }
            set
            {
                selectedInterval = value;
                base.NotifyPropertyChanged();

                SetFormatterY(value);
                LoadChart("ethbtc");
            }
        }
        #endregion

        public ChartService()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            OhclValues = new ChartValues<OhlcPoint>();
            LabelsX = new List<string>();
            FormatterY = value => Math.Round(value, 6).ToString() + "  ";

            Timeframes = KlineType.Intervals;
            SelectedInterval = Timeframes[0];

            Series = new SeriesCollection();
            CreateCandleSeries();
        }

        private void CreateLineSeries(ChartValues<double> values, string name)
        {
            dispatcher.InvokeAsync(() =>
            {
                Series.Add(new LineSeries()
                {
                    Values = values,
                    StrokeThickness = 2,
                    Stroke = Brushes.Green,
                    PointGeometry = null,
                    StrokeDashArray = new DoubleCollection() { 2 },
                    Fill = Brushes.Transparent,
                    Title = name
                });
            });
        }
        private void CreateCandleSeries()
        {
            CandleSeries = new CandleSeries();
            CandleSeries.Values = OhclValues;
            CandleSeries.StrokeThickness = 1;
            CandleSeries.Stroke = (Brush)new BrushConverter().ConvertFromString("#6BBA45");
            Series.Add(CandleSeries);
        }

        public void LoadChart(string pair)
        {
            OhclValues.Clear();
            LabelsX.Clear();
            isClose = false;

            if (kline != null)
            {
                kline.MessageEvent -= Kline_MessageEvent;
                kline = null;
            }
            kline = new Kline();
            kline.MessageEvent += Kline_MessageEvent;
            GetHistoryCandle(pair);
            kline.SocketOpen(pair, selectedInterval);
        }

        private void Kline_MessageEvent(object sender, KlineEventArgs e)
        {
            try
            {
                Candle candle = JConverter.JsonConver<Candle>(e.Message);
                var ohlcPoint = new OhlcPoint()
                {
                    Open = candle.k.o,
                    High = candle.k.h,
                    Low = candle.k.l,
                    Close = candle.k.c
                };

                if (isClose)
                {
                    OhclValues.Add(ohlcPoint);
                    LabelsX.Add(candle.k.t.ConvertUnixTime().ToString(formatX));
                    isClose = false;

                    // test
                    var values = new ChartValues<double>();
                    foreach (var item in OhclValues)
                    {
                        values.Add(0.0261);
                    }
                    CreateLineSeries(values, "123");
                    //------
                }
                else
                {
                    if (OhclValues.Count > 0)
                    {
                        var lastOhlc = OhclValues.Last();
                        lastOhlc.Open = ohlcPoint.Open;
                        lastOhlc.High = ohlcPoint.High;
                        lastOhlc.Low = ohlcPoint.Low;
                        lastOhlc.Close = ohlcPoint.Close;
                    }
                    else
                    {
                        OhclValues.Add(ohlcPoint);
                        LabelsX.Add(candle.k.t.ConvertUnixTime().ToString(formatX));
                    }
                }

                if (candle.k.x) // проверка должна быть после добавления, иначе добавим закрытую свечу
                {
                    isClose = true;
                }
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
        }

        private void GetHistoryCandle(string pair)
        {
            try
            {
                var klineString = kline.GetHistory(pair, selectedInterval);
                var klines = JConverter.JsonConver<List<object[]>>(klineString);

                foreach (var k in klines)
                {
                    var ohlcPoint = new OhlcPoint()
                    {
                        Open = Convert.ToDouble(k[1], new CultureInfo("en-US")),
                        High = Convert.ToDouble(k[2], new CultureInfo("en-US")),
                        Low = Convert.ToDouble(k[3], new CultureInfo("en-US")),
                        Close = Convert.ToDouble(k[4], new CultureInfo("en-US"))
                    };
                    OhclValues.Add(ohlcPoint);
                    LabelsX.Add(Convert.ToInt64(k[0]).ConvertUnixTime().ToString(formatX));
                }
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
        }

        private void SetFormatterY(string interval)
        {
            const string format1 = "HH:mm";
            const string format2 = "dd.MM.yyyy HH:mm";
            const string format3 = "dd.MM.yyyy";

            switch (interval)
            {
                case KlineType.m1:
                    formatX = format1;
                    break;
                case KlineType.m3:
                    formatX = format1;
                    break;
                case KlineType.m5:
                    formatX = format1;
                    break;
                case KlineType.m15:
                    formatX = format1;
                    break;
                case KlineType.m30:
                    formatX = format1;
                    break;
                case KlineType.h1:
                    formatX = format2;
                    break;
                case KlineType.h2:
                    formatX = format2;
                    break;
                case KlineType.h4:
                    formatX = format2;
                    break;
                case KlineType.h6:
                    formatX = format2;
                    break;
                case KlineType.h8:
                    formatX = format2;
                    break;
                case KlineType.h12:
                    formatX = format2;
                    break;
                case KlineType.d1:
                    formatX = format3;
                    break;
                case KlineType.w1:
                    formatX = format3;
                    break;
                case KlineType.M1:
                    formatX = format3;
                    break;
                default:
                    break;
            }
        }
    }
}
