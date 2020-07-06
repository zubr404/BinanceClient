using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Text;
using StockExchenge;
using Services;
using System.Linq;
using System.Collections.ObjectModel;

namespace BinanceClient.Services
{
    class ChartService : PropertyChangedBase
    {
        public ReadOnlyCollection<string> Timeframes { get; set; }
        public ChartValues<OhlcPoint> OhclValues { get; set; }
        public List<string> LabelsX { get; set; }
        public int AxisXStep { get; set; } = 30;
        public double AxisYStep { get; set; } = 0.00005;

        private readonly Kline kline;
        private bool isClose = false;   // признак, что предыдущая свеча закрыта

        private string selectedInterval;
        public string SelectedInterval
        {
            get { return selectedInterval; }
            set
            {
                selectedInterval = value;
                base.NotifyPropertyChanged();
            }
        }

        public ChartService()
        {
            AxisXStep = 30;
            Timeframes = KlineType.Intervals;
            SelectedInterval = Timeframes[0];
            OhclValues = new ChartValues<OhlcPoint>();
            LabelsX = new List<string>();
            kline = new Kline();
        }

        public void LoadChart(string pair)
        {
            GetHistoryCandle(pair);
            kline.SocketOpen(pair, selectedInterval);
            kline.WebSocket.OnMessage += WebSocket_OnMessage;
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
                }
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
        }
    }
}
