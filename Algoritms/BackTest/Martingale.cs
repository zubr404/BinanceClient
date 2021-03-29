using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Services;
using StockExchenge.MarketSettings;
using System.Globalization;
using StockExchenge.MarketTrades;
using Algoritms.Models;

namespace Algoritms.BackTest
{
    public class Martingale
    {
        const string LONG_STRATEGY = "Long";
        const string SHORT_STRATEGY = "Short";
        const string PUBLIC_KEY = "test_key";
        const string SECRET_KEY = "test_secret";
        private string simbol;

        readonly TradeHistoryRepository tradeHistoryRepository;
        readonly TradeConfigRepository tradeConfigRepository;
        readonly ExchangeInfo exchangeInfo;

        readonly Services.LogService logService;
        private BackTestConfiguration tradeConfiguration;
        private ExchangeSettingsPair exchangeSettingsPair;
        private StopLimitOrderRepository stopLimitOrderRepository;
        private TakeProfitOrderRepository takeProfitOrderRepository;
        private PositionRepository positionRepository;

        private bool IsActiveAlgoritm;
        public bool IsActiveCalculating { get; set; }

        public Martingale(TradeHistoryRepository tradeHistoryRepository, TradeConfigRepository tradeConfigRepository, ExchangeInfo exchangeInfo)
        {
            this.tradeHistoryRepository = tradeHistoryRepository;
            this.tradeConfigRepository = tradeConfigRepository;
            this.exchangeInfo = exchangeInfo;
            logService = new LogService();
        }

        private void SetConfifuration()
        {
            stopLimitOrderRepository = new StopLimitOrderRepository();
            takeProfitOrderRepository = new TakeProfitOrderRepository();
            positionRepository = new PositionRepository();

            tradeConfiguration = ToTestConfifuration(tradeConfigRepository.GetLast());
            simbol = CreateSimbolPair(tradeConfiguration.MainCoin, tradeConfiguration.AltCoin);
        }

        /// <summary>
        /// Запускает расчет для текущей сетки
        /// </summary>
        /// <param name="depositAsset"></param>
        /// <param name="depositQuote"></param>
        /// <returns></returns>
        public List<StopLimitOrderTest> CalcCurrentGrid(double depositAsset, double depositQuote)
        {
            logService.CreateLogFile("curretn_grid");
            SetConfifuration();
            tradeConfiguration.GeneralSetting.DepositAsset = depositAsset;
            tradeConfiguration.GeneralSetting.DepositQuote = depositQuote;
            return StartAlgoritm(-1);
        }

        public Statistics StartBackTest(DateTime startTime, DateTime stopTime, double depositAsset, double depositQuote)
        {
            logService.CreateLogFile("backtest");
            logService.Write("***********Нажата кнопка старт***********", true);

            SetConfifuration();
            tradeConfiguration.GeneralSetting.StartTime = TimeZoneInfo.ConvertTimeToUtc(startTime).ToUnixTime();
            tradeConfiguration.GeneralSetting.StopTime = TimeZoneInfo.ConvertTimeToUtc(stopTime).ToUnixTime();
            tradeConfiguration.GeneralSetting.DepositAsset = depositAsset;
            tradeConfiguration.GeneralSetting.DepositQuote = depositQuote;

            var startTradeId = tradeHistoryRepository.MinId(tradeConfiguration.GeneralSetting.StartTime);
            var stopTradeId = tradeHistoryRepository.MaxId(tradeConfiguration.GeneralSetting.StopTime);
            logService.Write($"startTradeId: {startTradeId} stopTradeId: {stopTradeId}");

            isReload = false;
            IsActiveAlgoritm = false;
            IsActiveCalculating = true;
            if (startTradeId > 0 && stopTradeId > 0)
            {
                var firstPrice = GetTades(simbol, startTradeId, startTradeId).FirstOrDefault()?.Price ?? 0;
                tradeConfiguration.GeneralSetting.SetDeposit(firstPrice);

                var amountElementsGet = 100000L;
                for (long i = startTradeId; i < stopTradeId; i += amountElementsGet)
                {
                    var stopI = i + amountElementsGet;
                    var trades = GetTades(simbol, i, stopI <= stopTradeId ? stopI : stopTradeId);

                    foreach (var trade in trades) // запуск теста. проходим по сделкам
                    {
                        if (!IsActiveAlgoritm)
                        {
                            StartAlgoritm(trade.Price);
                        }
                        else
                        {
                            CurrentTrades_LastPriceEvent(trade.Price);
                        }

                        long x = 10000;
                        if(trade.TradeId % x == 0)
                        {
                            logService.Write($"***********************************\n************************* {trade.TradeId} ************************\n*******************************************************", true);
                        }

                        if (!IsActiveAlgoritm)
                        {
                            logService.Write("***********TEST END** ERROR *********", true);
                            return null;
                        }
                        if (!IsActiveCalculating)
                        {
                            logService.Write("***********TEST END** Остановлено пользователем *********", true);
                            return null;
                        }
                    }
                }
            }

            var statistics = new Statistics(positionRepository.Get().ToList(), tradeConfiguration.GeneralSetting.Deposit);
            statistics.CalcStatistics();

            logService.Write("***********TEST END***********", true);
            return statistics;
        }

        //******************************************

        private bool isReload = false;
        /// <summary>
        /// Генерирует сетку ордеров. При отрицательном lastPrice получает текущую рыночную цену.
        /// </summary>
        /// <param name="lastPrice">цена рыночной сделки (если отрицательное - получает текущую рыночную цену)</param>
        /// <returns></returns>
        private List<StopLimitOrderTest> StartAlgoritm(double lastPrice)
        {
            logService.Write("----------------------Вызов StartAlgoritm----------------------", true);

            if(lastPrice < 0)
            {
                var lastPriceMarketTrade = new LastPriceMarketTrade();
                var priceResponse = lastPriceMarketTrade.GetInfo(simbol);
                if (priceResponse != null)
                {
                    lastPrice = priceResponse.price;
                }
                else
                {
                    IsActiveAlgoritm = false;
                    EndStartAlgoritm("Ошибка при получении цены последней сделки в блоке выставления начальной сетки. Алгоритм остановлен.");
                    return null;
                }
            }

            isReload = true;
            var orders = new List<StopLimitOrderTest>();

            if (tradeConfiguration != null)
            {
                logService.Write("++ получено tradeConfiguration");
            }
            else
            {
                EndStartAlgoritm("tradeConfiguration = null");
                return null;
            }

            try
            {
                GetExchangeInfo(exchangeInfo, simbol);
                logService.Write("++ выполнено GetExchangeInfo");
            }
            catch (Exception ex)
            {
                EndStartAlgoritm($"GetExchangeInfo Error: {ex.Message} InnerException: {ex.InnerException?.Message}");
                return null;
            }

            logService.Write($"tradeConfiguration: AltCoin:{tradeConfiguration.AltCoin} MainCoin:{tradeConfiguration.MainCoin} Strategy:{tradeConfiguration.Strategy} OrderDeposit:{tradeConfiguration.OrderDeposit} Martingale:{tradeConfiguration.Martingale} DepositLimit:{tradeConfiguration.DepositLimit} OrderIndent:{tradeConfiguration.OrderIndent} FirstStep:{tradeConfiguration.FirstStep} OrderStepPlus:{tradeConfiguration.OrderStepPlus} OrderReload:{tradeConfiguration.OrderReload} Loss:{tradeConfiguration.Loss} Profit:{tradeConfiguration.Profit} IndentExtremum:{tradeConfiguration.IndentExtremum} ProtectiveSpread:{tradeConfiguration.ProtectiveSpread}");

            logService.Write($"lastPrice: {lastPrice}");

            if (tradeConfiguration.Strategy == LONG_STRATEGY)
            {
                var freeBalance = tradeConfiguration.GeneralSetting.DepositQuote;
                var sumProfit = positionRepository.Get().ToList().Where(x => x.IsClose).Sum(x => x.Profit);
                freeBalance += sumProfit; // корректируем баланс с учетом закрытых позиций (только для лонгов)

                logService.Write($"sumProfit: {sumProfit}");
                logService.Write($"freeBalance: {freeBalance}");

                var allowedBalance = RoundAsset(GetAllowedBalance(freeBalance));
                logService.Write($"allowedBalance: {allowedBalance}");

                var stopPricePreviosOrder = RoundAsset(GetStopPriceFirstOrder(lastPrice, true));
                logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");

                var amountPreviosOrder = RoundLotSize(GetAmountFirstOrder(lastPrice));
                logService.Write($"amountPreviosOrder: {amountPreviosOrder}");

                orders.Add(CreateStopLimitOrder(PUBLIC_KEY, simbol, stopPricePreviosOrder, amountPreviosOrder, true));
                allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;
                logService.Write($"allowedBalance: {allowedBalance}");

                int counter = 0;
                while (true)
                {
                    //logService.Write($"while (true)----------------------");
                    stopPricePreviosOrder = RoundAsset(GetStopPriceNextOrder(stopPricePreviosOrder, true));
                    amountPreviosOrder = RoundLotSize(GetAmountNextOrder(amountPreviosOrder));
                    allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;

                    //logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");
                    //logService.Write($"amountPreviosOrder: {amountPreviosOrder}");
                    //logService.Write($"allowedBalance: {allowedBalance}");

                    if (allowedBalance < 0)
                    {
                        logService.Write($"while (end)----------------------");
                        break;
                    }

                    var avgStopPrice = GetAvgPriceOrders(orders);

                    if(tradeConfiguration.Loss > 0)
                    {
                        var stopLoss = RoundAsset(avgStopPrice - (avgStopPrice * tradeConfiguration.Loss / 100));
                        if (stopPricePreviosOrder < stopLoss)
                        {
                            logService.Write($"Цена стопа {stopPricePreviosOrder} ниже стоп-лосса {stopLoss}\nwhile (end)----------------------");
                            break;
                        }
                    }

                    orders.Add(CreateStopLimitOrder(PUBLIC_KEY, simbol, stopPricePreviosOrder, amountPreviosOrder, true));

                    if (counter > 999888)
                    {
                        IsActiveAlgoritm = false;
                        logService.Write($"Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                        logService.Write($"while (end)----------------------");
                        return null;
                    }
                    counter++;
                }
            }
            else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
            {
                var freeBalance = tradeConfiguration.GeneralSetting.DepositAsset;
                logService.Write($"freeBalance: {freeBalance}");

                var allowedBalance = RoundAsset(GetAllowedBalance(freeBalance));
                logService.Write($"allowedBalance: {allowedBalance}");

                var stopPricePreviosOrder = RoundAsset(GetStopPriceFirstOrder(lastPrice, false));
                logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");

                var amountPreviosOrder = RoundLotSize(GetAmountFirstOrder(lastPrice));
                logService.Write($"amountPreviosOrder: {amountPreviosOrder}");

                orders.Add(CreateStopLimitOrder(PUBLIC_KEY, simbol, stopPricePreviosOrder, amountPreviosOrder, false));
                allowedBalance -= amountPreviosOrder;
                logService.Write($"allowedBalance: {allowedBalance}");

                int counter = 0;
                while (true)
                {
                    //logService.Write($"while (true)----------------------");
                    stopPricePreviosOrder = RoundAsset(GetStopPriceNextOrder(stopPricePreviosOrder, false));
                    amountPreviosOrder = RoundLotSize(GetAmountNextOrder(amountPreviosOrder));
                    allowedBalance -= amountPreviosOrder;

                    //logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");
                    //logService.Write($"amountPreviosOrder: {amountPreviosOrder}");
                    //logService.Write($"allowedBalance: {allowedBalance}");

                    if (allowedBalance < 0)
                    {
                        //logService.Write($"while (end)----------------------");
                        break;
                    }

                    var avgStopPrice = GetAvgPriceOrders(orders);

                    if (tradeConfiguration.Loss > 0)
                    {
                        var stopLoss = RoundAsset(avgStopPrice - (avgStopPrice * tradeConfiguration.Loss / 100));
                        if (stopPricePreviosOrder > stopLoss)
                        {
                            logService.Write($"Цена стопа {stopPricePreviosOrder} выше стоп-лосса {stopLoss}\nwhile (end)----------------------");
                            break;
                        }
                    }

                    orders.Add(CreateStopLimitOrder(PUBLIC_KEY, simbol, stopPricePreviosOrder, amountPreviosOrder, false));

                    if (counter > 999888)
                    {
                        IsActiveAlgoritm = false;
                        logService.Write($"Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                        logService.Write($"while (end)----------------------");
                        return null;
                    }
                    counter++;
                }
            }

            foreach (var order in orders)
            {
                logService.Write($"Pair: {order.Pair} StopPrice: {order.StopPrice} Price: {order.Price} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active}");
            }

            stopLimitOrderRepository.Create(orders);
            logService.Write("Сетка ордеров создана.");
            IsActiveAlgoritm = true;

            EndStartAlgoritm();
            return orders;
        }

        private void EndStartAlgoritm(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                logService.Write($"StartAlgoritm message: {message}");
            }
            logService.Write("----------------------End StartAlgoritm----------------------", true);
        }

        /// <summary>
        /// Снятие стопов по всем счетам
        /// </summary>
        private void CancelAllStopOrders() // по всем счетам
        {
            stopLimitOrderRepository.DeactivationAllOrders();
        }

        /// <summary>
        /// Снятие тайк-профитов по всем счетам
        /// </summary>
        private void CancelAllTakeProfitOrder() // по всем счетам
        {
            takeProfitOrderRepository.DeactivationAllOrders();
        }

        private void CurrentTrades_LastPriceEvent(double lastPrice)
        {
            if (IsActiveAlgoritm)
            {
                //logService.Write("------------------CurrentTrades_LastPriceEvent------------------");
                //logService.Write($"currentPair.Pair: {simbol} lastPrice: {lastPrice} IsActiveAlgoritm: {IsActiveAlgoritm} Strategy: {tradeConfiguration.Strategy}");

                if (tradeConfiguration.Strategy == LONG_STRATEGY)
                {
                    // отслеживание OrderReload
                    var maxPrice = 0d;
                    try
                    {
                        maxPrice = stopLimitOrderRepository.MaxStopPriceBuy;
                    }
                    catch (Exception ex)
                    {
                        logService.Write($"maxPrice: error - {ex.Message}");
                        EndTask();
                        return;
                    }
                    //logService.Write($"maxPrice: {maxPrice}");

                    var indent = 0.0;
                    if (maxPrice > 0)
                    {
                        indent = ((lastPrice - maxPrice) * 100) / maxPrice;
                    }
                    //logService.Write($"indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");

                    if (indent > tradeConfiguration.OrderReload && tradeConfiguration.OrderReload > 0 && isReload)
                    {
                        stopLimitOrderRepository.DeactivationAllOrders();
                        takeProfitOrderRepository.DeactivationAllOrders();
                        StartAlgoritm(lastPrice);

                        logService.Write($"Перемещение сетки OrderReload. indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");
                        EndTask();
                        return;
                    }

                    // отслеживание исполнения стопов
                    //logService.Write("---- отслеживание исполнения стопов");
                    var stopOrders = stopLimitOrderRepository.GetActive(simbol).OrderByDescending(x => x.StopPrice);

                    foreach (var stopOrder in stopOrders)
                    {
                        //OnMessageDebugEvent("отслеживание исполнения стопов");
                        try
                        {
                            //logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                            if (lastPrice <= stopOrder.StopPrice)
                            {
                                // get secret key
                                var publicKey = PUBLIC_KEY;
                                var secretKey = SECRET_KEY;

                                if (stopOrder.IsBuyOperation)
                                {
                                    logService.Write($"Сработал стоп на открытие позиции: stopOrder.Pair: {stopOrder.Pair} stopOrder.IsBuyOperation: {stopOrder.IsBuyOperation} stopOrder.Amount: {stopOrder.Amount} ** lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    isReload = false; // если есть иполнение запрещаем перестановку ордеров

                                    // обновляем стоп
                                    stopLimitOrderRepository.DeactivateOrder(stopOrder.ID);
                                    // выставляеим на Бинансе
                                    var position = positionRepository.GetOpen().FirstOrDefault();
                                    if(position != null)
                                    {
                                        position.IcreasePosition(stopOrder.Amount, lastPrice);
                                    }
                                    else
                                    {
                                        position = positionRepository.Create(new Position(stopOrder.Pair, stopOrder.Amount, lastPrice, isLong: true));
                                    }

                                    // снимаем все стопы-loss и профиты
                                    stopLimitOrderRepository.DeactivationAllOrders(publicKey, false);
                                    takeProfitOrderRepository.DeactivationAllOrders(publicKey);

                                    // выставляем профит
                                    var profitStop = new TakeProfitOrderTest()
                                    {
                                        FK_PublicKey = publicKey,
                                        Pair = stopOrder.Pair,
                                        StopPrice = RoundAsset(position.Price + (position.Price * tradeConfiguration.Profit / 100)),
                                        IndentExtremum = tradeConfiguration.IndentExtremum,
                                        ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                                        Amount = RoundLotSize(Math.Abs(position.Amount)),
                                        IsBuyOperation = false,
                                        Active = true
                                    };
                                    takeProfitOrderRepository.Create(profitStop);
                                    logService.Write("выставляем профит");
                                    logService.Write($"Pair: {profitStop.Pair} StopPrice: {profitStop.StopPrice} IndentExtremum: {profitStop.IndentExtremum} ProtectiveSpread: {profitStop.ProtectiveSpread} Amount: {profitStop.Amount} IsBuyOperation: {profitStop.IsBuyOperation} Active: {profitStop.Active}");

                                    // выставляем лосс
                                    var lossStop = new StopLimitOrderTest()
                                    {
                                        FK_PublicKey = publicKey,
                                        Pair = stopOrder.Pair,
                                        StopPrice = RoundAsset(position.Price - (position.Price * tradeConfiguration.Loss / 100)),
                                        Price = 0,
                                        Amount = RoundLotSize(Math.Abs(position.Amount)),
                                        IsBuyOperation = false,
                                        Active = true
                                    };
                                    stopLimitOrderRepository.Create(lossStop);
                                    logService.Write("выставляем лосс");
                                    logService.Write($"Pair: {lossStop.Pair} StopPrice: {lossStop.StopPrice} Price: {lossStop.Price} Amount: {lossStop.Amount} IsBuyOperation: {lossStop.IsBuyOperation} Active: {lossStop.Active}");
                                }
                                else // сработал стоп-лосс
                                {
                                    logService.Write($"Сработал стоп-лосс. stopOrder.Pair: {stopOrder.Pair} stopOrder.IsBuyOperation: {stopOrder.IsBuyOperation} stopOrder.Amount: {stopOrder.Amount} ** lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    // выставляеим на Бинансе
                                    var position = positionRepository.GetOpen().FirstOrDefault();
                                    if (position != null)
                                    {
                                        position.ClosePosition(lastPrice);
                                    }
                                    else
                                    {
                                        // error
                                    }
                                    // все снимаем
                                    stopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                    takeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                    // начинаем заново
                                    StartAlgoritm(lastPrice);
                                    EndTask();
                                    return;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: loging
                            logService.Write($"System error: {ex.Message}");
                        }
                    }

                    // отслеживание тейк-профитов
                    //logService.Write($"---- отслеживание тейк-профитов");
                    var takeProfits = takeProfitOrderRepository.GetActive(simbol).OrderBy(x => x.StopPrice);

                    //logService.Write($"foreach (var order in takeProfits) I -----------------");
                    foreach (var order in takeProfits)
                    {
                        //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                        //logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {order.StopPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                        if (lastPrice >= order.StopPrice && order.ExtremumPrice <= 0)
                        {
                            // update ExtremumPrice
                            //logService.Write($"Initialize ExtremumPrice: {lastPrice}");
                            takeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                        }
                    }
                    //logService.Write($"end I -----------------");

                    takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderByDescending(x => x.ExtremumPrice);

                    //logService.Write($"foreach (var order in takeProfits) II -----------------");
                    foreach (var order in takeProfits)
                    {
                        // get secret key
                        var publicKey = PUBLIC_KEY;
                        var secretKey = SECRET_KEY;

                        var indentExtremum = ((order.ExtremumPrice - lastPrice) * 100) / order.ExtremumPrice;
                        //OnMessageDebugEvent($"IE:{indentExtremum} * OIE:{order.IndentExtremum}");
                        //logService.Write($"indentExtremum: {indentExtremum} order.IndentExtremum: {order.IndentExtremum}");

                        if (indentExtremum >= order.IndentExtremum)
                        {
                            logService.Write($"Сработал тейк-профит: Pair: {order.Pair} StopPrice: {order.StopPrice} IndentExtremum: {order.IndentExtremum} ProtectiveSpread: {order.ProtectiveSpread} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active} ** lastPrice: {lastPrice}");

                            // выставляеим на Бинансе
                            var position = positionRepository.GetOpen().FirstOrDefault();
                            if (position != null)
                            {
                                position.ClosePosition(lastPrice);
                            }
                            else
                            {
                                // error
                            }
                            // все снимаем
                            stopLimitOrderRepository.DeactivationAllOrders(publicKey);
                            takeProfitOrderRepository.DeactivationAllOrders(publicKey);
                            // начинаем заново
                            StartAlgoritm(lastPrice);
                            EndTask();
                            return;
                        }

                        //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                        //logService.Write($"Update ExtremumPrice");
                        //logService.Write($"lastPrice: {lastPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                        if (lastPrice > order.ExtremumPrice && order.ExtremumPrice > 0)
                        {
                            // update ExtremumPrice
                            takeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                        }
                    }
                    //logService.Write($"end II -----------------");
                    //----
                }

                //-----------------------------------------
                //----------- SHORT -----------------------
                //-----------------------------------------

                else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
                {
                    //OnMessageDebugEvent("LONG_STRATEGY");
                    // отслеживание OrderReload
                    var minPrice = 0d;
                    try
                    {
                        minPrice = stopLimitOrderRepository.MinStopPriceSell;
                    }
                    catch (Exception ex)
                    {
                        logService.Write($"minPrice: error - {ex.Message}");
                        EndTask();
                        return;
                    }
                    //logService.Write($"minPrice: {minPrice}");

                    var indent = 0.0;
                    if (minPrice > 0)
                    {
                        indent = ((minPrice - lastPrice) * 100) / minPrice;
                    }
                    //logService.Write($"indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");

                    if (indent > tradeConfiguration.OrderReload && tradeConfiguration.OrderReload > 0 && isReload)
                    {
                        stopLimitOrderRepository.DeactivationAllOrders();
                        takeProfitOrderRepository.DeactivationAllOrders();
                        StartAlgoritm(lastPrice);

                        logService.Write($"Перемещение сетки OrderReload. indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");
                        EndTask();
                        return;
                    }

                    // отслеживание исполнения стопов
                    //logService.Write("---- отслеживание исполнения стопов");
                    var stopOrders = stopLimitOrderRepository.GetActive(simbol).OrderBy(x => x.StopPrice);

                    foreach (var stopOrder in stopOrders)
                    {
                        //OnMessageDebugEvent("отслеживание исполнения стопов");
                        try
                        {
                            //logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                            if (lastPrice >= stopOrder.StopPrice)
                            {
                                logService.Write($"lastPrice: {lastPrice}");
                                // get secret key
                                var publicKey = PUBLIC_KEY;
                                var secretKey = SECRET_KEY;

                                if (!stopOrder.IsBuyOperation)
                                {
                                    logService.Write($"Сработал стоп на открытие позиции: stopOrder.Pair: {stopOrder.Pair} stopOrder.IsBuyOperation: {stopOrder.IsBuyOperation} stopOrder.Amount: {stopOrder.Amount} ** lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    isReload = false; // если есть иполнение запрещаем перестановку ордеров

                                    // обновляем стоп
                                    stopLimitOrderRepository.DeactivateOrder(stopOrder.ID);
                                    // выставляеим на Бинансе
                                    var position = positionRepository.GetOpen().FirstOrDefault();
                                    if (position != null)
                                    {
                                        position.IcreasePosition(stopOrder.Amount, lastPrice);
                                    }
                                    else
                                    {
                                        position = positionRepository.Create(new Position(stopOrder.Pair, stopOrder.Amount, lastPrice, isLong: false));
                                    }
                                    // снимаем все стопы-loss и профиты
                                    stopLimitOrderRepository.DeactivationAllOrders(publicKey, !stopOrder.IsBuyOperation);
                                    takeProfitOrderRepository.DeactivationAllOrders(publicKey);

                                    // выставляем профит
                                    var profitStop = CreateTakeProfitOrder(publicKey, stopOrder.Pair, position.Price, position.Amount, isBuyer: true);
                                    takeProfitOrderRepository.Create(profitStop);
                                    logService.Write("выставляем профит");
                                    logService.Write($"Pair: {profitStop.Pair} StopPrice: {profitStop.StopPrice} IndentExtremum: {profitStop.IndentExtremum} ProtectiveSpread: {profitStop.ProtectiveSpread} Amount: {profitStop.Amount} IsBuyOperation: {profitStop.IsBuyOperation} Active: {profitStop.Active}");

                                    // выставляем лосс
                                    double stopPrice = GetStopLossPrice(position.Price, tradeConfiguration.Loss, !stopOrder.IsBuyOperation);
                                    var lossStop = CreateStopLimitOrder(publicKey, stopOrder.Pair, stopPrice, RoundLotSize(Math.Abs(position.Amount)), true);
                                    stopLimitOrderRepository.Create(lossStop);
                                    logService.Write("выставляем лосс");
                                    logService.Write($"Pair: {lossStop.Pair} StopPrice: {lossStop.StopPrice} Price: {lossStop.Price} Amount: {lossStop.Amount} IsBuyOperation: {lossStop.IsBuyOperation} Active: {lossStop.Active}");
                                }
                                else // сработал стоп-лосс
                                {
                                    logService.Write($"Сработал стоп-лосс. ** lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    // выставляеим на Бинансе
                                    var position = positionRepository.GetOpen().FirstOrDefault();
                                    if (position != null)
                                    {
                                        position.ClosePosition(lastPrice);
                                    }
                                    else
                                    {
                                        // error
                                    }
                                    // все снимаем
                                    stopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                    takeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                    // начинаем заново
                                    StartAlgoritm(lastPrice);
                                    EndTask();
                                    return;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: loging
                            logService.Write($"System error: {ex.Message}");
                        }
                    }

                    // отслеживание тейк-профитов
                    //logService.Write($"---- отслеживание тейк-профитов");
                    var takeProfits = takeProfitOrderRepository.GetActive(simbol).OrderByDescending(x => x.StopPrice);

                    //logService.Write($"foreach (var order in takeProfits) I -----------------");
                    foreach (var order in takeProfits)
                    {
                        //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                        //logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {order.StopPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                        if (lastPrice <= order.StopPrice && order.ExtremumPrice <= 0)
                        {
                            // update ExtremumPrice
                            //logService.Write($"Initialize ExtremumPrice: {lastPrice}");
                            takeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                        }
                    }
                    //logService.Write($"end I -----------------");

                    takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderBy(x => x.ExtremumPrice);

                    //logService.Write($"foreach (var order in takeProfits) II -----------------");
                    foreach (var order in takeProfits)
                    {
                        // get secret key
                        var publicKey = PUBLIC_KEY;
                        var secretKey = SECRET_KEY;

                        var indentExtremum = ((lastPrice - order.ExtremumPrice) * 100) / order.ExtremumPrice;
                        //OnMessageDebugEvent($"IE:{indentExtremum} * OIE:{order.IndentExtremum}");
                        //logService.Write($"indentExtremum: {indentExtremum} order.IndentExtremum: {order.IndentExtremum}");

                        if (indentExtremum >= order.IndentExtremum)
                        {
                            logService.Write($"Сработал тейк-профит: Pair: {order.Pair} StopPrice: {order.StopPrice} IndentExtremum: {order.IndentExtremum} ProtectiveSpread: {order.ProtectiveSpread} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active} ** lastPrice: {lastPrice}");

                            // выставляеим на Бинансе
                            var position = positionRepository.GetOpen().FirstOrDefault();
                            if (position != null)
                            {
                                position.ClosePosition(lastPrice);
                            }
                            else
                            {
                                // error
                            }
                            // все снимаем
                            stopLimitOrderRepository.DeactivationAllOrders(publicKey);
                            takeProfitOrderRepository.DeactivationAllOrders(publicKey);
                            // начинаем заново
                            StartAlgoritm(lastPrice);
                            EndTask();
                            return;
                        }

                        //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                        //logService.Write($"Update ExtremumPrice");
                        //logService.Write($"lastPrice: {lastPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                        if (lastPrice < order.ExtremumPrice && order.ExtremumPrice > 0)
                        {
                            // update ExtremumPrice
                            takeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                        }
                    }
                    //logService.Write($"end II -----------------");
                    //----
                }
                EndTask();
            }
        }

        private void EndTask()
        {
            //logService.Write($"***  end TASK ***", true);
        }

        private double GetAvgPriceOrders(List<StopLimitOrderTest> orders)
        {
            double result = 0;
            try
            {
                if (orders != null)
                {
                    var sumMoney = Math.Round(orders.Sum(x => x.StopPrice * x.Amount), 10);
                    var sumAmount = Math.Round(orders.Sum(x => x.Amount), 10);
                    result = (double)Math.Round(((decimal)sumMoney / (decimal)sumAmount), 10);
                }
            }
            catch (Exception)
            {

            }
            return result;
        }

        private double GetStopPriceFirstOrder(double lastPrice, bool isBuyer)
        {
            if (isBuyer)
            {
                return lastPrice - (lastPrice * tradeConfiguration.OrderIndent / 100);
            }
            else
            {
                return lastPrice + (lastPrice * tradeConfiguration.OrderIndent / 100);
            }
        }

        private double GetStopPriceNextOrder(double stopPricePreviosOrder, bool isBuyer)
        {
            if (isBuyer)
            {
                return stopPricePreviosOrder - ((stopPricePreviosOrder * tradeConfiguration.FirstStep / 100) + (stopPricePreviosOrder * tradeConfiguration.OrderStepPlus / 100));
            }
            else
            {
                return stopPricePreviosOrder + ((stopPricePreviosOrder * tradeConfiguration.FirstStep / 100) + (stopPricePreviosOrder * tradeConfiguration.OrderStepPlus / 100));
            }
        }

        private double GetStopLossPrice(double price, double lossParam, bool isBuyer)
        {
            if (!isBuyer)
            {
                return price - (price * lossParam / 100);
            }
            else
            {
                return price + (price * lossParam / 100);
            }
        }

        private double GetAmountFirstOrder(double lastPrice)
        {
            return tradeConfiguration.OrderDeposit / lastPrice;
        }

        private double GetAmountNextOrder(double amountPreviosOrder)
        {
            return amountPreviosOrder + (amountPreviosOrder * tradeConfiguration.Martingale / 100);
        }

        private StopLimitOrderTest CreateStopLimitOrder(string key, string pair, double price, double amount, bool isBuyer)
        {
            return new StopLimitOrderTest()
            {
                FK_PublicKey = key,
                Pair = pair,
                StopPrice = price,
                Price = 0,
                Amount = amount,
                IsBuyOperation = isBuyer,
                Active = true
            };
        }

        private TakeProfitOrderTest CreateTakeProfitOrder(string key, string pair, double price, double amount, bool isBuyer)
        {
            if (!isBuyer)
            {
                return new TakeProfitOrderTest()
                {
                    FK_PublicKey = key,
                    Pair = pair,
                    StopPrice = RoundAsset(price + (price * tradeConfiguration.Profit / 100)),
                    IndentExtremum = tradeConfiguration.IndentExtremum,
                    ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                    Amount = RoundLotSize(Math.Abs(amount)),
                    IsBuyOperation = isBuyer,
                    Active = true
                };
            }
            else
            {
                return new TakeProfitOrderTest()
                {
                    FK_PublicKey = key,
                    Pair = pair,
                    StopPrice = RoundAsset(price - (price * tradeConfiguration.Profit / 100)),
                    IndentExtremum = tradeConfiguration.IndentExtremum,
                    ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                    Amount = RoundLotSize(Math.Abs(amount)),
                    IsBuyOperation = isBuyer,
                    Active = true
                };
            }
        }

        private double GetOrderLimit(double balance)
        {
            return balance * tradeConfiguration.OrderDeposit / 100;
        }
        private double GetAllowedBalance(double freeBalance)
        {
            return freeBalance * tradeConfiguration.DepositLimit / 100;
        }

        private double RoundBaseAsset(double value)
        {
            return Math.Round(value, exchangeSettingsPair.BasePrecision);
        }
        private double RoundAsset(double value)
        {
            return Math.Round(value, exchangeSettingsPair.QuotePrecision);
        }

        private double RoundLotSize(double value)
        {
            decimal size = (decimal)value;
            int sizeInt = (int)(size / exchangeSettingsPair.LotSizeFilter.StepSize);
            decimal roundSize = sizeInt * exchangeSettingsPair.LotSizeFilter.StepSize;
            return (double)roundSize;
        }

        private void GetExchangeInfo(ExchangeInfo exchangeInfo, string simbol)
        {
            exchangeSettingsPair = new ExchangeSettingsPair();
            try
            {
                var jsonString = exchangeInfo.Info;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    if (symbol.symbol == simbol)
                    {
                        exchangeSettingsPair.QuotePrecision = symbol.quotePrecision;
                        exchangeSettingsPair.BasePrecision = symbol.baseAssetPrecision;
                        foreach (var filter in symbol.filters)
                        {
                            if (filter.filterType == "LOT_SIZE")
                            {
                                decimal stepSize;
                                decimal.TryParse(filter.stepSize.ToString(), NumberStyles.Number, new CultureInfo("en-US"), out stepSize);
                                exchangeSettingsPair.LotSizeFilter.StepSize = stepSize;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        struct AvgPricePositionResult
        {
            public double SumAmount { get; set; }
            public double AvgPrice { get; set; }
        }

        //******************************************


        private BackTestConfiguration ToTestConfifuration(TradeConfiguration configuration)
        {
            return new BackTestConfiguration()
            {
                MainCoin = configuration.MainCoin,
                AltCoin = configuration.AltCoin,
                Strategy = configuration.Strategy,
                Margin = configuration.Margin,
                OrderIndent = configuration.OrderIndent,
                OrderDeposit = configuration.OrderDeposit,
                FirstStep = configuration.FirstStep,
                OrderStepPlus = configuration.OrderStepPlus,
                Martingale = configuration.Martingale,
                DepositLimit = configuration.DepositLimit,
                OrderReload = configuration.OrderReload,
                Loss = configuration.Loss,
                Profit = configuration.Profit,
                IndentExtremum = configuration.IndentExtremum,
                ProtectiveSpread = configuration.ProtectiveSpread
            };
        }

        private IEnumerable<TradeHistory> GetTades(string pair, long startTradeId, long stopTradeId)
        {
            return tradeHistoryRepository.Get(pair, startTradeId, stopTradeId);
        }

        private string CreateSimbolPair(string baseCoin, string altCoin)
        {
            return $"{baseCoin}{altCoin}".ToUpper();
        }
    }
}
