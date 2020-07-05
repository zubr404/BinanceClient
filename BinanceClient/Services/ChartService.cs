using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Text;
using StockExchenge;
using Services;
using System.Linq;

namespace BinanceClient.Services
{
    class ChartService
    {
        public ChartValues<OhlcPoint> OhclValues { get; set; }
        public List<string> LabelsX { get; set; }
        public int AxisXStep { get; set; } = 30;

        private readonly Kline kline;

        public ChartService()
        {
            OhclValues = new ChartValues<OhlcPoint>();
            LabelsX = new List<string>();
            kline = new Kline();
        }

        public void LoadChart(string pair, string interval)
        {
            kline.SocketOpen(pair, interval);
            kline.WebSocket.OnMessage += WebSocket_OnMessage;
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            try
            {
                Candle candle = JConverter.JsonConver<Candle>(e.Data);
                var ohlcPoint = new OhlcPoint()
                {
                    Open = candle.k.o,
                    High = candle.k.h,
                    Low = candle.k.l,
                    Close = candle.k.c
                };

                if (candle.k.x)
                {
                    OhclValues.Add(ohlcPoint);
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
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
            
        }
    }
}
