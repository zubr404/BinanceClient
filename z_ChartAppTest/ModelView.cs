using Services;
using StockExchenge.Charts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using z_ChartAppTest.Models;
using z_ChartAppTest.Services;

namespace z_ChartAppTest
{
    class ModelView
    {
        public double GridHeight { get; private set; }
        public double GridWidth { get; private set; }
        public CandlestickService CandlestickService { get; private set; }

        private Kline kline;
        private List<Candle> candles;

        private Dispatcher dispatcher;
        private Timer timer;

        public ModelView()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            GridHeight = 800;
            GridWidth = 1150;
            CandlestickService = new CandlestickService();

            kline = new Kline("BTCUSDT", "1m");
            candles = new List<Candle>();

            timer = new Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ChartStart();
        }

        private async Task ChartStart()
        {
            await Task.Run(() =>
            {
                GetHistoryCandle();
                dispatcher.InvokeAsync(() =>
                {
                    CandlestickService.GetMaxAllChart(candles);
                    CandlestickService.GetMinAllChart(candles);
                    CandlestickService.GetDeltaAllChart(candles);
                    CandlestickService.GetScaleIntervalPrice(GridHeight);
                    CandlestickService.CreateChart(candles);
                });
            });
        }

        private void GetHistoryCandle()
        {
            try
            {
                candles.Clear();
                var klineString = kline.GetHistory();
                var klines = JConverter.JsonConver<List<object[]>>(klineString);

                foreach (var k in klines)
                {
                    var ohlcPoint = new Candle(Convert.ToDouble(k[2], new CultureInfo("en-US")),
                        Convert.ToDouble(k[3], new CultureInfo("en-US")),
                        Convert.ToDouble(k[1], new CultureInfo("en-US")),
                        Convert.ToDouble(k[4], new CultureInfo("en-US")));
                    candles.Add(ohlcPoint);
                }
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
        }
    }
}
