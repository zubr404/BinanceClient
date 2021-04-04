using Charts.Services;
using Services;
using StockExchenge;
using StockExchenge.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace BinanceClient.Services
{
    public class ChartService : PropertyChangedBase
    {
        public ReadOnlyCollection<string> Timeframes { get; set; }
        public double GridHeight { get; private set; }
        public double GridWidth { get; private set; }

        public Charts.Services.ChartService ChartServ { get; private set; }

        private Kline kline;
        private List<Charts.Models.Candle> candles;
        private Candlestick candlestick;

        private Dispatcher dispatcher;
        private Timer timer;
        private int quotePrecision;

        public ChartService(Dispatcher dispatcher, double gridHeight, double gridWidth)
        {
            this.dispatcher = dispatcher;
            GridHeight = gridHeight;
            GridWidth = gridWidth;
            candlestick = new Candlestick();
            ChartServ = new Charts.Services.ChartService(candlestick);

            candles = new List<Charts.Models.Candle>();

            Timeframes = KlineType.Intervals;
            SelectedInterval = Timeframes.First();

            timer = new Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        #region Properties
        private string selectedInterval = "1m";
        public string SelectedInterval
        {
            get { return selectedInterval; }
            set
            {
                selectedInterval = value;
                base.NotifyPropertyChanged();
            }
        }
        private string selectedPair = PairsMy.Pairs.First();
        public string SelectedPair
        {
            get { return selectedPair; }
            set
            {
                selectedPair = value;
                base.NotifyPropertyChanged();
            }
        }
        #endregion

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ChartStart();
        }

        private async Task ChartStart()
        {
            await Task.Run(() =>
            {
                GetHistoryCandle();
                GetExchangeInfo();
                dispatcher.InvokeAsync(() =>
                {
                    candlestick.SetCandles(candles);
                    ChartServ.ChartBuild(candles, SelectedPair, GridHeight, GridWidth, candles.Last().Close, quotePrecision);
                });
            });
        }

        private void GetExchangeInfo()
        {
            try
            {
                var jsonString = MainWindow.ExchangeInfo.Info;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    if (symbol.symbol == selectedPair)
                    {
                        quotePrecision = symbol.quotePrecision;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                // TODO: запись лога в БД
            }
        }

        private void GetHistoryCandle()
        {
            try
            {
                candles.Clear();
                kline = new Kline(SelectedPair, SelectedInterval);
                var klineString = kline.GetHistory();
                var klines = JConverter.JsonConver<List<object[]>>(klineString);

                foreach (var k in klines)
                {
                    var ohlcPoint = new Charts.Models.Candle(
                        Convert.ToInt64(k[0], new CultureInfo("en-US")),
                        Convert.ToDouble(k[2], new CultureInfo("en-US")),
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
