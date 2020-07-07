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

namespace BinanceClient.Services
{
    class ChartService : PropertyChangedBase
    {
        public ReadOnlyCollection<string> Timeframes { get; set; }
        public ChartValues<OhlcPoint> OhclValues { get; set; }
        public List<string> LabelsX { get; set; }
        public Func<double, string> FormatterY { get; set; }
        public int AxisXStep { get; set; } = 30;

        private Kline kline;
        private bool isClose = false;   // признак, что предыдущая свеча закрыта
        private string formatX = "";

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

        public ChartService()
        {
            OhclValues = new ChartValues<OhlcPoint>();
            LabelsX = new List<string>();
            FormatterY = value => Math.Round(value, 6).ToString() + "  ";
            kline = new Kline();
            //
            kline.MessageEvent += Kline_MessageEvent;

            Timeframes = KlineType.Intervals;
            SelectedInterval = Timeframes[0];
        }

        public void LoadChart(string pair)
        {
            OhclValues.Clear();
            LabelsX.Clear();
            isClose = false;

            GetHistoryCandle(pair);
            kline.SocketOpen(pair, selectedInterval);
            kline.WebSocket.OnMessage += WebSocket_OnMessage;
        }

        private void Kline_MessageEvent(object sender, KlineEventArgs e)
        {
            
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            try
            {
                Candle candle = JConverter.JsonConver<Candle>(e.Data);
                var ohlcPoint = new OhlcPoint()
                {
                    Open = Math.Round(candle.k.o, 6),
                    High = Math.Round(candle.k.h, 6),
                    Low = Math.Round(candle.k.l, 6),
                    Close = Math.Round(candle.k.c, 6)
                };

                if (isClose)
                {
                    OhclValues.Add(ohlcPoint);
                    isClose = false;
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
                        Open = Math.Round(Convert.ToDouble(k[1].ToString().Replace(".",",")), 6),
                        High = Math.Round(Convert.ToDouble(k[2].ToString().Replace(".", ",")), 6),
                        Low = Math.Round(Convert.ToDouble(k[3].ToString().Replace(".", ",")), 6),
                        Close = Math.Round(Convert.ToDouble(k[4].ToString().Replace(".", ",")), 6)
                    };
                    OhclValues.Add(ohlcPoint);

                    var x = Convert.ToInt64(k[0]);
                    var y = x.ConvertUnixTime();
                    var z = y.ToString();
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
