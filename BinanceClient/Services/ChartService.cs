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
using System.Threading.Tasks;
using StockExchenge.Charts;
using System.Threading;
using DataBaseWork.Repositories;
using DataBaseWork.Models;

namespace BinanceClient.Services
{
    class ChartService : PropertyChangedBase
    {
        /*
         Вынос собственных сделок на график:
        можно проверять какое-нибудь хранилище (скорее всего БД)
        при обновлении сокета по свече
        или
        днлать запрос на биржу о собственных сделках.
        Сокет скорее всего сюда по сделкам лепить не буду.
         */
        public ReadOnlyCollection<string> Timeframes { get; set; }
        public List<string> PairSelecteds { get; set; }
        public ChartValues<OhlcPoint> OhclValues { get; set; }
        public List<string> LabelsX { get; set; }
        public Func<double, string> FormatterY { get; set; }
        public SeriesCollection Series { get; set; }
        public CandleSeries CandleSeries { get; set; }


        private Kline kline;
        private bool isClose = false;   // признак, что предыдущая свеча закрыта
        private string formatX = "";
        private int quotePrecision;
        private string pair = "";

        readonly Dispatcher dispatcher;

        private RelayCommand refrashChart;
        public RelayCommand RefrashChart
        {
            get
            {
                return refrashChart ?? new RelayCommand((object o) =>
                {
                    LoadChart(pair);
                });
            }
        }

        #region Properties
        private string selectedInterval;
        public string SelectedInterval
        {
            get { return selectedInterval; }
            set
            {
                selectedInterval = value;
                base.NotifyPropertyChanged();

                SetFormatterX(value);

                Task.Run(() =>
                {
                    GetExchangeInfo();
                    LoadChart(pair);
                });
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

                SetFormatterX(selectedInterval);

                Task.Run(() =>
                {
                    GetExchangeInfo();
                    //LoadChart(value);
                });
            }
        }
        #endregion

        public ChartService(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            OhclValues = new ChartValues<OhlcPoint>();
            LabelsX = new List<string>();

            Timeframes = KlineType.Intervals;
            SelectedInterval = Timeframes.First();
            PairSelecteds = PairsMy.Pairs;

            Series = new SeriesCollection();
            CreateCandleSeries();
        }

        #region добавляет линии на график
        private void CreateLineSeries(ChartValues<double> values, bool isBuyer, string name)
        {
            dispatcher.InvokeAsync(() =>
            {
                SolidColorBrush brush = Brushes.Red;
                if (isBuyer)
                {
                    brush = Brushes.Green;
                }
                if(!Series.Any(x=>x.Title == name))
                {
                    Series.Add(new LineSeries()
                    {
                        Values = values,
                        StrokeThickness = 1,
                        Stroke = brush,
                        PointGeometry = null,
                        StrokeDashArray = new DoubleCollection() { 4 },
                        Fill = Brushes.Transparent,
                        Title = name
                    });
                }
            });
        }

        private async Task<List<TradeLine>> GetTrades(string simbol, double minPrice, double maxPrice)
        {
            var result = new List<TradeLine>();
            await Task.Run(() =>
            {
                var tradeRepository = new TradeRepository();
                var trades = tradeRepository.Get(simbol, minPrice, maxPrice).ToList();
                var resGroup = trades.GroupBy(x => new {x.Price, x.IsBuyer }).ToList();
                foreach (var item in resGroup)
                {
                    result.Add(new TradeLine()
                    {
                        Price = item.Key.Price,
                        IsBuyer = item.Key.IsBuyer
                    });
                }
            });
            return result;
        }

        private async Task CreateChartValuesLines()
        {
            await Task.Run(async () =>
            {
                var trades = await GetTrades(pair, OhclValues.Min(x => x.Low), OhclValues.Max(x => x.High));

                foreach (var trade in trades)
                {
                    var values = new ChartValues<double>();
                    for (int i = 0; i < OhclValues.Count; i++)
                    {
                        values.Add(trade.Price);
                    }
                    CreateLineSeries(values, trade.IsBuyer, trade.Price.ToString());
                }
            });
        }
        #endregion

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
            this.pair = pair;
            var formatPrecision = PrecisionFormatting();
            FormatterY = value => Math.Round(value, quotePrecision).ToString(formatPrecision); // настроить величину оеругления в зависимоти от спецификации инструмента
            isClose = false;

            if (kline != null)
            {
                kline.MessageEvent -= Kline_MessageEvent;
                kline.ConnectStateEvent -= Kline_ConnectStateEvent;
                kline.ConnectEvent -= Kline_ConnectEvent;
                kline = null;
            }
            kline = new Kline(pair, selectedInterval);
            kline.MessageEvent += Kline_MessageEvent;
            kline.ConnectStateEvent += Kline_ConnectStateEvent;
            kline.ConnectEvent += Kline_ConnectEvent;
            kline.SocketOpen();
        }

        private void Kline_ConnectEvent(object sender, EventArgs e)
        {
            OhclValues.Clear();
            LabelsX.Clear();
            GetHistoryCandle();
        }

        private void Kline_ConnectStateEvent(object sender, string e)
        {
            ModelView.ConsoleScrin1.Message = e;
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

                var firstCandle = OhclValues.FirstOrDefault();
                if (isClose)
                {
                    OhclValues.Add(ohlcPoint);
                    OhclValues.Remove(firstCandle);
                    LabelsX.Add(candle.k.t.UnixToDateTime().ToString(formatX));
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
                        LabelsX.Add(candle.k.t.UnixToDateTime().ToString(formatX));
                    }
                }

                if (candle.k.x) // проверка должна быть после добавления, иначе добавим закрытую свечу
                {
                    isClose = true;
                }
            }
            catch (Exception ex)
            {
                //TODO: запись логов в БД
            }
            //CreateChartValuesLines();
        }

        private void GetHistoryCandle()
        {
            try
            {
                var klineString = kline.GetHistory();
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
                    LabelsX.Add(Convert.ToInt64(k[0]).UnixToDateTime().ToString(formatX));
                }
            }
            catch (Exception ex)
            {
                // запись логов в БД
            }
        }

        private void SetFormatterX(string interval)
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


        #region Пример ответа
        /*
         {
  "timezone": "UTC",
  "serverTime": 1508631584636,
  "rateLimits": [{
      "rateLimitType": "REQUESTS",
      "interval": "MINUTE",
      "limit": 1200
    },
    {
      "rateLimitType": "ORDERS",
      "interval": "SECOND",
      "limit": 10
    },
    {
      "rateLimitType": "ORDERS",
      "interval": "DAY",
      "limit": 100000
    }
  ],
  "exchangeFilters": [],
  "symbols": [{
    "symbol": "ETHBTC",
    "status": "TRADING",
    "baseAsset": "ETH",
    "baseAssetPrecision": 8,
    "quoteAsset": "BTC",
    "quotePrecision": 8,
    "orderTypes": ["LIMIT", "MARKET"],
    "icebergAllowed": false,
    "filters": [{
      "filterType": "PRICE_FILTER",
      "minPrice": "0.00000100",
      "maxPrice": "100000.00000000",
      "tickSize": "0.00000100"
    }, {
      "filterType": "LOT_SIZE",
      "minQty": "0.00100000",
      "maxQty": "100000.00000000",
      "stepSize": "0.00100000"
    }, {
      "filterType": "MIN_NOTIONAL",
      "minNotional": "0.00100000"
    }]
  }]
}
         */
        #endregion
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

        private string PrecisionFormatting()
        {
            var result = "0.#";
            for (int i = 0; i < quotePrecision - 1; i++)
            {
                result += "#";
            }
            return result;
        }
    }
}
