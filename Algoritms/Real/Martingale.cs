using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using StockExchenge.MarketTrades;
using DataBaseWork.Models;
using System.Linq;
using Services;
using System.Globalization;
using StockExchenge.MarketSettings;
using StockExchenge.Transaction;
using System.Threading.Tasks;
using System.Threading;
using StockExchenge.TradeAccount;
using DataBaseWork;
using Algoritms.Models;
using Algoritms.MappingModel;

namespace Algoritms.Real
{
    public class Martingale
    {
        const string LONG_STRATEGY = "Long";
        const string SHORT_STRATEGY = "Short";

        readonly RepositoriesM repositoriesM;
        public bool IsActiveAlgoritm { get; set; }

        private TradeConfigurator tradeConfigurator;
        private List<APIKey> apiKeys;

        public event EventHandler<string> MessageErrorEvent;
        public event EventHandler<string> MessageDebugEvent;

        private bool isReload;

        private Services.LogService logService;

        public Martingale(RepositoriesM repositoriesM)
        {
            this.repositoriesM = repositoriesM;
            this.repositoriesM.CurrentTrades.LastPriceEvent += CurrentTrades_LastPriceEvent;

            tradeConfigurator = new TradeConfigurator();

            MessageErrorEvent = delegate { };
            MessageDebugEvent = delegate { };
            logService = new LogService();
        }

        public void StopAlgoritm()
        {
            IsActiveAlgoritm = false;
            // снимаем все стопы и профиты
            CancelAllStopOrders();
            CancelAllTakeProfitOrder();
        }

        /// <summary>
        /// Общий запуск по всем конфигам
        /// </summary>
        public void StartAlgoritm()
        {
            if (!IsActiveAlgoritm)
            {
                logService.CreateLogFile();
                logService.Write("***********Нажата кнопка старт***********", true);
            }

            logService.Write("----------------------Вызов StartAlgoritm----------------------", true);

            isReload = true;

            // ------------ получаем сохраненные конфиги ----------
            var configsDb = repositoriesM.TradeConfigRepository.GetActive();
            if (configsDb != null)
            {
                logService.Write("++ получено tradeConfiguration");
                var configs = configsDb.ToTradeConfigurationAlgos();
                tradeConfigurator.SetConfigurations(configs);
            }
            else
            {
                EndStartAlgoritm("tradeConfiguration = null");
                return;
            }
            //-----------------------------------------------

            // -------- получаем ключи -----------
            apiKeys = repositoriesM.APIKeyRepository.GetActiveStatusOk().ToList();
            if (apiKeys == null)
            {
                EndStartAlgoritm("apiKeys = null");
                return;
            }
            else
            {
                if(apiKeys.Count == 0)
                {
                    EndStartAlgoritm("apiKeys.Count == 0");
                    return;
                }
                else
                {
                    logService.Write("++ выполнено apiKeys");
                }
            }
            //-----------------------------------------------

            // ############ ВЫСТАВЛЯЕМ СЕТКИ ПО КОНФИГАМ ######################
            foreach (var keyValue in tradeConfigurator.TradeConfigurations)
            {
                var tradeConfiguration = keyValue.Value;
                StartAlgoritm(tradeConfiguration);
            }
            
            IsActiveAlgoritm = true;
            EndStartAlgoritm();
        }

        /// <summary>
        /// Зпуск по одному конфигу
        /// </summary>
        /// <param name="tradeConfiguration"></param>
        private void StartAlgoritm(TradeConfigurationAlgo tradeConfiguration)
        {
            var pair = tradeConfiguration.GetPair();
            double lastPrice;
            var orders = new List<StopLimitOrder>();

            logService.Write($"tradeConfiguration: AltCoin:{tradeConfiguration.AltCoin} MainCoin:{tradeConfiguration.MainCoin} Strategy:{tradeConfiguration.Strategy} OpenOrders:{tradeConfiguration.OpenOrders} OrderDeposit:{tradeConfiguration.OrderDeposit} Martingale:{tradeConfiguration.Martingale} DepositLimit:{tradeConfiguration.DepositLimit} OrderIndent:{tradeConfiguration.OrderIndent} FirstStep:{tradeConfiguration.FirstStep} OrderStepPlus:{tradeConfiguration.OrderStepPlus} OrderReload:{tradeConfiguration.OrderReload} Loss:{tradeConfiguration.Loss} Profit:{tradeConfiguration.Profit} IndentExtremum:{tradeConfiguration.IndentExtremum} ProtectiveSpread:{tradeConfiguration.ProtectiveSpread} Active:{tradeConfiguration.Active}  ActivationTime:{tradeConfiguration.ActivationTime}  DeactivationTime:{tradeConfiguration.DeactivationTime}");

            // -------- получаем ExchangeInfo -----------
            ExchangeSettingsPair exchangeSettingsPair;
            try
            {
                exchangeSettingsPair = GetExchangeInfo(pair);
                logService.Write("++ выполнено GetExchangeInfo");
            }
            catch (Exception ex)
            {
                EndStartAlgoritm($"GetExchangeInfo Error: {ex.Message} InnerException: {ex.InnerException?.Message}");
                return;
            }
            //-----------------------------------------------

            // получаем цену последней сделки
            var lastPriceMarketTrade = new LastPriceMarketTrade();
            var priceResponse = lastPriceMarketTrade.GetInfo(pair);
            if (priceResponse != null)
            {
                lastPrice = priceResponse.price;
            }
            else
            {
                OnMessageErrorEvent("Ошибка при получении цены последней сделки в блоке выставления начальной сетки. Алгоритм остановлен.");
                IsActiveAlgoritm = false;
                EndStartAlgoritm("Ошибка при получении цены последней сделки в блоке выставления начальной сетки. Алгоритм остановлен.");
                return;
            }
            logService.Write($"lastPrice: {lastPrice}");
            //---------------------------------------


            var existsStopLimit = repositoriesM.StopLimitOrderRepository.ExistsActive(pair);
            var existsTakeProfit = repositoriesM.TakeProfitOrderRepository.ExistsActive(pair);

            logService.Write($"existsStopLimit: {existsStopLimit} existsTakeProfit: {existsTakeProfit}");

            // ################# ПРОХОДИМ ПО ВСЕМ СЧЕТАМ ########################
            if (!existsStopLimit && !existsTakeProfit)
            {
                foreach (var key in apiKeys)
                {
                    var countSimbolKey = key.PublicKey.Length - 6;
                    logService.Write($"Сетка для ключа: {key.PublicKey.Substring(countSimbolKey > 0 ? countSimbolKey : key.PublicKey.Length)}; name: {key.Name}; status: {key.Status} Стратегия: {tradeConfiguration.Strategy}");

                    // -------------- ЛОНГИ ---------------
                    if (tradeConfiguration.Strategy == LONG_STRATEGY)
                    {
                        var balance = GetBalanceQuoteAsset(repositoriesM.BalanceRepository, key.PublicKey, tradeConfiguration.AltCoin);
                        var freeBalance = 0.0;
                        if (balance != null)
                        {
                            freeBalance = balance.Free;
                            logService.Write($"freeBalance: {freeBalance}");
                        }

                        if (freeBalance == 0)
                        {
                            continue;
                        }
                        var allowedBalance = RoundQuoteAsset(GetAllowedBalance(freeBalance, tradeConfiguration.DepositLimit), exchangeSettingsPair.QuotePrecision);
                        logService.Write($"allowedBalance: {allowedBalance}");

                        var stopPricePreviosOrder = RoundQuoteAsset(GetStopPriceFirstOrder(true, lastPrice, tradeConfiguration.OrderIndent), exchangeSettingsPair.QuotePrecision);
                        logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");

                        var amountPreviosOrder = RoundLotSize(GetAmountFirstOrder(tradeConfiguration.OrderDeposit, lastPrice), exchangeSettingsPair.LotSizeFilter.StepSize);
                        logService.Write($"amountPreviosOrder: {amountPreviosOrder}");

                        allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;

                        if (allowedBalance > 0)
                        {
                            orders.Add(CreateStopLimitOrder(key.PublicKey, pair, stopPricePreviosOrder, amountPreviosOrder, true));
                            logService.Write($"allowedBalance: {allowedBalance}");

                            int counter = 0;
                            while (true)
                            {
                                logService.Write($"while (true)----------------------");
                                stopPricePreviosOrder = RoundQuoteAsset(GetStopPriceNextOrder(stopPricePreviosOrder, tradeConfiguration.FirstStep, tradeConfiguration.OrderStepPlus, true), exchangeSettingsPair.QuotePrecision);
                                amountPreviosOrder = RoundLotSize(GetAmountNextOrder(amountPreviosOrder, tradeConfiguration.Martingale), exchangeSettingsPair.LotSizeFilter.StepSize);
                                allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;

                                logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");
                                logService.Write($"amountPreviosOrder: {amountPreviosOrder}");
                                logService.Write($"allowedBalance: {allowedBalance}");

                                if (allowedBalance < 0)
                                {
                                    logService.Write($"while (end)----------------------");
                                    break;
                                }

                                var avgStopPrice = GetAvgPriceOrders(orders);

                                if (tradeConfiguration.Loss > 0)
                                {
                                    var stopLoss = RoundQuoteAsset(avgStopPrice - (avgStopPrice * tradeConfiguration.Loss / 100), exchangeSettingsPair.QuotePrecision);
                                    if (stopPricePreviosOrder < stopLoss)
                                    {
                                        logService.Write($"Цена стопа {stopPricePreviosOrder} ниже стоп-лосса {stopLoss}\nwhile (end)----------------------");
                                        break;
                                    }
                                }

                                orders.Add(CreateStopLimitOrder(key.PublicKey, pair, stopPricePreviosOrder, amountPreviosOrder, true));

                                if (counter > 999888)
                                {
                                    IsActiveAlgoritm = false;
                                    OnMessageErrorEvent("Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                                    logService.Write($"Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                                    logService.Write($"while (end)----------------------");
                                    return;
                                }
                                counter++;
                            }
                        }
                    }

                    // -------------- ШОРТЫ ---------------
                    else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
                    {
                        var balance = GetBalanceBaseAsset(repositoriesM.BalanceRepository, key.PublicKey, tradeConfiguration.MainCoin);
                        var freeBalance = 0.0;
                        if (balance != null)
                        {
                            freeBalance = balance.Free;
                            logService.Write($"freeBalance: {freeBalance}");
                        }
                        if (freeBalance == 0)
                        {
                            continue;
                        }
                        var allowedBalance = RoundQuoteAsset(GetAllowedBalance(freeBalance, tradeConfiguration.DepositLimit), exchangeSettingsPair.QuotePrecision);
                        logService.Write($"allowedBalance: {allowedBalance}");

                        var stopPricePreviosOrder = RoundQuoteAsset(GetStopPriceFirstOrder(false, lastPrice, tradeConfiguration.OrderIndent), exchangeSettingsPair.QuotePrecision);
                        logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");

                        var amountPreviosOrder = RoundLotSize(GetAmountFirstOrder(tradeConfiguration.OrderDeposit, lastPrice), exchangeSettingsPair.LotSizeFilter.StepSize);
                        logService.Write($"amountPreviosOrder: {amountPreviosOrder}");

                        allowedBalance -= amountPreviosOrder;

                        if (allowedBalance > 0)
                        {
                            orders.Add(CreateStopLimitOrder(key.PublicKey, pair, stopPricePreviosOrder, amountPreviosOrder, false));
                            logService.Write($"allowedBalance: {allowedBalance}");

                            int counter = 0;
                            while (true)
                            {
                                logService.Write($"while (true)----------------------");
                                stopPricePreviosOrder = RoundQuoteAsset(GetStopPriceNextOrder(stopPricePreviosOrder, tradeConfiguration.FirstStep, tradeConfiguration.OrderStepPlus, false), exchangeSettingsPair.QuotePrecision);
                                amountPreviosOrder = RoundLotSize(GetAmountNextOrder(amountPreviosOrder, tradeConfiguration.Martingale), exchangeSettingsPair.LotSizeFilter.StepSize);
                                allowedBalance -= amountPreviosOrder;

                                logService.Write($"stopPricePreviosOrder: {stopPricePreviosOrder}");
                                logService.Write($"amountPreviosOrder: {amountPreviosOrder}");
                                logService.Write($"allowedBalance: {allowedBalance}");

                                if (allowedBalance < 0)
                                {
                                    logService.Write($"while (end)----------------------");
                                    break;
                                }

                                var avgStopPrice = GetAvgPriceOrders(orders);

                                if (tradeConfiguration.Loss > 0)
                                {
                                    var stopLoss = RoundQuoteAsset(avgStopPrice - (avgStopPrice * tradeConfiguration.Loss / 100), exchangeSettingsPair.QuotePrecision);
                                    if (stopPricePreviosOrder > stopLoss)
                                    {
                                        logService.Write($"Цена стопа {stopPricePreviosOrder} выше стоп-лосса {stopLoss}\nwhile (end)----------------------");
                                        break;
                                    }
                                }

                                orders.Add(CreateStopLimitOrder(key.PublicKey, pair, stopPricePreviosOrder, amountPreviosOrder, false));

                                if (counter > 999888)
                                {
                                    IsActiveAlgoritm = false;
                                    OnMessageErrorEvent("Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                                    logService.Write($"Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                                    logService.Write($"while (end)----------------------");
                                    return;
                                }
                                counter++;
                            }
                        }
                    }
                }

                foreach (var order in orders)
                {
                    logService.Write($"Key: {order.FK_PublicKey.Substring(order.FK_PublicKey.Length - 6)} Pair: {order.Pair} StopPrice: {order.StopPrice} Price: {order.Price} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active}");
                }

                // Обновляем статус ключей
                //foreach (var key in apiKeys)
                //{
                //    repositoriesM.APIKeyRepository.UpdateStatus(key.PublicKey, true);
                //}
                var publicKeys = apiKeys.Select(x => x.PublicKey);
                var publicKeysOrders = orders.Select(x => x.FK_PublicKey).Distinct();
                var exceptKeys = publicKeys.Except(publicKeysOrders);
                foreach (var key in exceptKeys)
                {
                    repositoriesM.APIKeyRepository.UpdateStatus(key, false);
                }
                //-------------------------

                repositoriesM.StopLimitOrderRepository.Create(orders);
                OnMessageDebugEvent($"Сетка ордеров создана. Pair: {pair}");
                logService.Write($"Сетка ордеров создана. Pair: {pair}");
            }
            else
            {
                isReload = false;
                OnMessageDebugEvent($"Сетка ордеров уже имеется. Pair: {pair}");
                logService.Write($"Сетка ордеров уже имеется. Pair: {pair}");
            }
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
        public void CancelAllStopOrders() // по всем счетам
        {
            repositoriesM.StopLimitOrderRepository.DeactivationAllOrders();
        }

        /// <summary>
        /// Снятие тайк-профитов по всем счетам
        /// </summary>
        public void CancelAllTakeProfitOrder() // по всем счетам
        {
            repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders();
        }


        //>>>>>>>>>>>>>>>>>>>>>>>> CurrentTrades_LastPriceEvent <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        Task task;
        private void CurrentTrades_LastPriceEvent(object sender, LastPriceEventArgs e)
        {
            if (task != null)
            {
                if (!task.IsCompleted)
                {
                    return;
                }
                else
                {
                    if (task.IsFaulted)
                    {
                        logService.Write($"CurrentTrades_LastPriceEvent ERROR: Exception: {task.Exception.Message}\nInnerException: {task.Exception.InnerException?.Message}\nStackTrace: {task.Exception.StackTrace}", true);
                        OnMessageDebugEvent($"CurrentTrades_LastPriceEvent ERROR: Exception: {task.Exception.Message}");
                        CancelAllStopOrders();
                        OnMessageDebugEvent($"Алгоритм остановлен. Все заявки по всем счетам отменены.");
                        IsActiveAlgoritm = false;
                        task = null;
                    }
                }
            }

            if (IsActiveAlgoritm)
            {
                var pair = tradeConfigurator.TradeConfigurations.Keys.FirstOrDefault(x => x.ToUpper() == e.Pair.ToUpper());
                if (!string.IsNullOrWhiteSpace(pair))
                {
                    var tradeConfiguration = tradeConfigurator.TradeConfigurations[pair];
                    task = Task.Run(() =>
                    {
                        bool isExecutionAnyOrder = false; // если есть исполнение хотя бы одного тейка или лосса

                        //OnMessageDebugEvent(task.Id.ToString());
                        logService.Write($"*** START TASK {task.Id} ***", true);

                        var lastPrice = e.LastPrice;
                        //OnMessageDebugEvent(lastPrice.ToString());
                        logService.Write("------------------CurrentTrades_LastPriceEvent------------------");
                        logService.Write($"currentPair.Pair: {pair} lastPrice: {lastPrice} IsActiveAlgoritm: {IsActiveAlgoritm} Strategy: {tradeConfiguration.Strategy}");

                        var db = new DataBaseContext();
                        var apiKeypository = new APIKeyRepository();
                        var balanceRepository = new BalanceRepository();
                        var tradeConfigRepository = new TradeConfigRepository();
                        var stopLimitRepository = new StopLimitOrderRepository();
                        var takeProfitRepository = new TakeProfitOrderRepository();
                        var tradeRepository = new TradeRepository();

                        // -------- получаем ExchangeInfo -----------
                        ExchangeSettingsPair exchangeSettingsPair;
                        try
                        {
                            exchangeSettingsPair = GetExchangeInfo(pair);
                        }
                        catch (Exception)
                        {
                            EndTask();
                            throw;
                        }
                        //-----------------------------------------------

                        //**************************************** LONG_STRATEGY ****************************************
                        if (tradeConfiguration.Strategy == LONG_STRATEGY)
                        {
                            //OnMessageDebugEvent("LONG_STRATEGY");
                            // отслеживание OrderReload
                            var maxPrice = 0d;
                            try
                            {
                                maxPrice = stopLimitRepository.GetMaxStopPriceBuy(pair);
                            }
                            catch (Exception ex)
                            {
                                logService.Write($"maxPrice: error - {ex.Message}");
                                EndTask();
                                throw ex;
                            }
                            logService.Write($"maxPrice: {maxPrice}");

                            var indent = 0.0;
                            if (maxPrice > 0)
                            {
                                indent = ((lastPrice - maxPrice) * 100) / maxPrice;
                            }
                            logService.Write($"indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");

                            if (indent > tradeConfiguration.OrderReload && tradeConfiguration.OrderReload > 0 && isReload)
                            {
                                OnMessageDebugEvent("Перемещение сетки OrderReload.");
                                stopLimitRepository.DeactivationForPair(pair);
                                takeProfitRepository.DeactivationForPair(pair);
                                StartAlgoritm(tradeConfiguration);

                                logService.Write("Перемещение сетки OrderReload.");
                                EndTask();
                                return;
                            }

                            // отслеживание исполнения стопов
                            logService.Write("---- отслеживание исполнения стопов");
                            var stopOrders = stopLimitRepository.GetStopOrdersFilledBuy(pair, lastPrice).OrderByDescending(x => x.StopPrice);

                            double stopPriceForTakeProfit = 0; // что бы по всем счетам поставить ТП с одинаковой ценой
                            double stopPriceForStopLoss = 0; // что бы по всем счетам поставить СЛ с одинаковой ценой
                            double stopPriceLast = 0; // запоминаем предыдущую стоп-цену

                            foreach (var stopOrder in stopOrders)
                            {
                                if (stopPriceLast != stopOrder.StopPrice && stopPriceLast != 0) // это на случай если цена пробила сразу несколько заявок в глубину
                                {
                                    logService.Write("---- цена пробила сразу несколько заявок в глубину");
                                    stopPriceForTakeProfit = 0;
                                    stopPriceForStopLoss = 0;
                                }
                                stopPriceLast = stopOrder.StopPrice;

                                //OnMessageDebugEvent("отслеживание исполнения стопов");
                                try
                                {
                                    logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    if (lastPrice <= stopOrder.StopPrice)
                                    {
                                        // get secret key
                                        var publicKey = stopOrder.FK_PublicKey;
                                        var secretKey = apiKeypository.GetSecretKey(publicKey);
                                        var nameKey = apiKeypository.GetNameKey(publicKey);

                                        if (stopOrder.IsBuyOperation)
                                        {
                                            logService.Write($"Сработал стоп на открытие позиции: stopOrder.Pair: {stopOrder.Pair} stopOrder.IsBuyOperation: {stopOrder.IsBuyOperation} stopOrder.Amount: {stopOrder.Amount}");
                                            isReload = false; // если есть иполнение запрещаем перестановку ордеров

                                            // обновляем стоп
                                            stopLimitRepository.DeactivateOrder(stopOrder.ID);
                                            // выставляеим на Бинансе
                                            var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);

                                            var messageError = "";
                                            if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                            {
                                                OnMessageDebugEvent($"Открытие позиции: УСПЕШНО. Ключ: {nameKey}");
                                                logService.Write($"Открытие позиции: УСПЕШНО. Ключ: {nameKey}");
                                                // запрашиваем сделки с биржи
                                                if (!RequestedTrades(publicKey, secretKey, pair, long.Parse(orderResponse.OrderId.Trim()), (decimal)stopOrder.Amount))
                                                {
                                                    OnMessageDebugEvent("Не удалось получить последние сделки по счету.");
                                                    logService.Write("RequestedTrades: FAILED");
                                                    EndTask();
                                                    return;
                                                }
                                                // считаем среднюю цену позы
                                                var getAvgResult = GetAvgPricePosition(tradeRepository, tradeConfiguration.ActivationTime, publicKey, pair, !stopOrder.IsBuyOperation);
                                                logService.Write($"getAvgResult.AvgPrice: {getAvgResult.AvgPrice} getAvgResult.SumAmount: {getAvgResult.SumAmount}");

                                                // снимаем все стопы-loss и профиты
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair, false);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                if (stopPriceForTakeProfit <= 0)
                                                {
                                                    stopPriceForTakeProfit = RoundBaseAsset(getAvgResult.AvgPrice + (getAvgResult.AvgPrice * tradeConfiguration.Profit / 100), exchangeSettingsPair.BasePrecision);
                                                }
                                                logService.Write($"stopPriceForTakeProfit: {stopPriceForTakeProfit}");

                                                // выставляем профит
                                                var profitStop = new TakeProfitOrder()
                                                {
                                                    FK_PublicKey = publicKey,
                                                    Pair = stopOrder.Pair,
                                                    StopPrice = stopPriceForTakeProfit,
                                                    IndentExtremum = tradeConfiguration.IndentExtremum,
                                                    ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                                                    Amount = RoundLotSize(Math.Abs(getAvgResult.SumAmount), exchangeSettingsPair.LotSizeFilter.StepSize),
                                                    IsBuyOperation = false,
                                                    Active = true
                                                };
                                                takeProfitRepository.Create(profitStop);
                                                logService.Write("выставляем профит");
                                                logService.Write($"Key: {nameKey} Pair: {profitStop.Pair} StopPrice: {profitStop.StopPrice} IndentExtremum: {profitStop.IndentExtremum} ProtectiveSpread: {profitStop.ProtectiveSpread} Amount: {profitStop.Amount} IsBuyOperation: {profitStop.IsBuyOperation} Active: {profitStop.Active}");

                                                // выставляем лосс
                                                if (tradeConfiguration.Loss > 0)
                                                {
                                                    if (stopPriceForStopLoss <= 0)
                                                    {
                                                        stopPriceForStopLoss = RoundBaseAsset(getAvgResult.AvgPrice - (getAvgResult.AvgPrice * tradeConfiguration.Loss / 100), exchangeSettingsPair.BasePrecision);
                                                    }
                                                    logService.Write($"stopPriceForStopLoss: {stopPriceForStopLoss}");

                                                    var lossStop = new StopLimitOrder()
                                                    {
                                                        FK_PublicKey = publicKey,
                                                        Pair = stopOrder.Pair,
                                                        StopPrice = stopPriceForStopLoss,
                                                        Price = 0,
                                                        Amount = RoundLotSize(Math.Abs(getAvgResult.SumAmount), exchangeSettingsPair.LotSizeFilter.StepSize),
                                                        IsBuyOperation = false,
                                                        Active = true
                                                    };
                                                    stopLimitRepository.Create(lossStop);
                                                    logService.Write("выставляем лосс");
                                                    logService.Write($"Key: {nameKey} Pair: {lossStop.Pair} StopPrice: {lossStop.StopPrice} Price: {lossStop.Price} Amount: {lossStop.Amount} IsBuyOperation: {lossStop.IsBuyOperation} Active: {lossStop.Active}");
                                                }
                                            }
                                            else
                                            {
                                                OnMessageDebugEvent($"Открытие позиции: ОШИБКА. Ключ: {nameKey}");
                                                logService.Write($"Открытие позиции: Ключ: {nameKey} ОШИБКА - {messageError}");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                //IsActiveAlgoritm = false;
                                                //EndTask();
                                                //return;

                                                apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                            }
                                        }
                                        else // сработал стоп-лосс
                                        {
                                            logService.Write("Сработал стоп-лосс");
                                            // выставляеим на Бинансе
                                            var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);

                                            var messageError = "";
                                            if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                            {
                                                OnMessageDebugEvent("Стоп-лосс на Бинансе: УСПЕШНО");
                                                logService.Write("Стоп-лосс на Бинансе: УСПЕШНО");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                                // начинаем заново
                                                //StartAlgoritm();
                                                //EndTask();
                                                //return;
                                                isExecutionAnyOrder = true;
                                            }
                                            else
                                            {
                                                OnMessageErrorEvent($"ВНИМАНИЕ! Стоп-лосс ошибка! Ключ: {nameKey}");
                                                logService.Write($"ВНИМАНИЕ! Стоп-лосс ошибка Ключ: {nameKey} Error: {messageError}");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                //IsActiveAlgoritm = false;
                                                //EndTask();
                                                //return;

                                                apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                            }
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
                                    OnMessageDebugEvent($"System error 1: {ex.Message}");
                                    logService.Write($"System error 1: {ex.Message}");
                                    throw ex;
                                }
                            }

                            try
                            {
                                // отслеживание тейк-профитов
                                logService.Write($"---- отслеживание тейк-профитов");
                                var takeProfits = takeProfitRepository.GetActive(pair).OrderBy(x => x.StopPrice);

                                logService.Write($"foreach (var order in takeProfits) I -----------------");
                                foreach (var order in takeProfits)
                                {
                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {order.StopPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                                    if (lastPrice >= order.StopPrice && order.ExtremumPrice <= 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Initialize ExtremumPrice: {lastPrice}");
                                        logService.Write($"Initialize ExtremumPrice: {lastPrice}");
                                        takeProfitRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }
                                logService.Write($"end I -----------------");

                                takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderByDescending(x => x.ExtremumPrice);

                                logService.Write($"foreach (var order in takeProfits) II -----------------");
                                foreach (var order in takeProfits)
                                {
                                    var indentExtremum = ((order.ExtremumPrice - lastPrice) * 100) / order.ExtremumPrice;
                                    //OnMessageDebugEvent($"IE:{indentExtremum} * OIE:{order.IndentExtremum}");
                                    logService.Write($"indentExtremum: {indentExtremum} order.IndentExtremum: {order.IndentExtremum}");

                                    if (indentExtremum >= order.IndentExtremum)
                                    {
                                        // get secret key
                                        var publicKey = order.FK_PublicKey;
                                        var secretKey = apiKeypository.GetSecretKey(publicKey);
                                        var nameKey = apiKeypository.GetNameKey(publicKey);

                                        logService.Write($"Сработал тейк-профит: Key: {nameKey} Pair: {order.Pair} StopPrice: {order.StopPrice} IndentExtremum: {order.IndentExtremum} ProtectiveSpread: {order.ProtectiveSpread} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active}");

                                        // выставляеим на Бинансе
                                        var orderResponse = SendOrder(order.Pair, order.IsBuyOperation, order.Amount, publicKey, secretKey);

                                        var messageError = "";
                                        if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                        {
                                            OnMessageDebugEvent($"Тейк-профит: УСПЕШНО Ключ: {nameKey}");
                                            logService.Write($"Тейк-профит: УСПЕШНО Ключ: {nameKey}");
                                            // все снимаем
                                            stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все стоп-лимиты. {publicKey}");
                                            takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все тейк-профиты. {publicKey}");
                                            // начинаем заново
                                            //StartAlgoritm();
                                            //EndTask();
                                            //return;

                                            isExecutionAnyOrder = true;
                                        }
                                        else
                                        {
                                            OnMessageErrorEvent($"ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Ключ: {nameKey}");
                                            logService.Write($"ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Ключ: {nameKey} Error: {messageError}");

                                            // все снимаем
                                            stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все стоп-лимиты. {publicKey}");
                                            takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все тейк-профиты. {publicKey}");

                                            //IsActiveAlgoritm = false;
                                            //EndTask();
                                            //return;

                                            apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                        }
                                    }

                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    logService.Write($"Update ExtremumPrice");
                                    logService.Write($"lastPrice: {lastPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                                    if (lastPrice > order.ExtremumPrice && order.ExtremumPrice > 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Update ExtremumPrice: {lastPrice}");
                                        takeProfitRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }
                                logService.Write($"end II -----------------");
                                //----
                            }
                            catch (Exception ex)
                            {
                                // TODO: loging
                                OnMessageDebugEvent($"System error 2: {ex.Message}");
                                logService.Write($"System error 2: {ex.Message}");
                                throw ex;
                            }
                        }
                        //**************************************** SHORT_STRATEGY ****************************************
                        else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
                        {
                            //OnMessageDebugEvent("LONG_STRATEGY");
                            // отслеживание OrderReload
                            var minPrice = 0d;
                            try
                            {
                                minPrice = stopLimitRepository.GetMinStopPriceSell(pair);
                            }
                            catch (Exception ex)
                            {
                                logService.Write($"minPrice: error - {ex.Message}");
                                EndTask();
                                throw ex;
                            }
                            logService.Write($"minPrice: {minPrice}");

                            var indent = 0.0;
                            if (minPrice > 0)
                            {
                                indent = ((minPrice - lastPrice) * 100) / minPrice;
                            }
                            logService.Write($"indent: {indent} tradeConfiguration.OrderReload: {tradeConfiguration.OrderReload} isReload: {isReload}");

                            if (indent > tradeConfiguration.OrderReload && tradeConfiguration.OrderReload > 0 && isReload)
                            {
                                OnMessageDebugEvent("Перемещение сетки OrderReload.");
                                stopLimitRepository.DeactivationForPair(pair);
                                takeProfitRepository.DeactivationForPair(pair);
                                StartAlgoritm(tradeConfiguration);

                                logService.Write("Перемещение сетки OrderReload.");
                                EndTask();
                                return;
                            }

                            // отслеживание исполнения стопов
                            logService.Write("---- отслеживание исполнения стопов");
                            var stopOrders = stopLimitRepository.GetStopOrdersFilledSell(pair, lastPrice).OrderBy(x => x.StopPrice);

                            double stopPriceForTakeProfit = 0; // что бы по всем счетам поставить ТП с одинаковой ценой
                            double stopPriceForStopLoss = 0; // что бы по всем счетам поставить СЛ с одинаковой ценой
                            double stopPriceLast = 0; // запоминаем предыдущую стоп-цену

                            foreach (var stopOrder in stopOrders)
                            {
                                if (stopPriceLast != stopOrder.StopPrice && stopPriceLast != 0) // это на случай если цена пробила сразу несколько заявок в глубину
                                {
                                    logService.Write("---- цена пробила сразу несколько заявок в глубину");
                                    stopPriceForTakeProfit = 0;
                                    stopPriceForStopLoss = 0;
                                }
                                stopPriceLast = stopOrder.StopPrice;

                                //OnMessageDebugEvent("отслеживание исполнения стопов");
                                try
                                {
                                    logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {stopOrder.StopPrice}");
                                    if (lastPrice >= stopOrder.StopPrice)
                                    {
                                        // get secret key
                                        var publicKey = stopOrder.FK_PublicKey;
                                        var secretKey = apiKeypository.GetSecretKey(publicKey);
                                        var nameKey = apiKeypository.GetNameKey(publicKey);

                                        if (!stopOrder.IsBuyOperation)
                                        {
                                            logService.Write($"Сработал стоп на открытие позиции: stopOrder.Pair: {stopOrder.Pair} stopOrder.IsBuyOperation: {stopOrder.IsBuyOperation} stopOrder.Amount: {stopOrder.Amount}");
                                            isReload = false; // если есть иполнение запрещаем перестановку ордеров

                                            // обновляем стоп
                                            stopLimitRepository.DeactivateOrder(stopOrder.ID);
                                            // выставляеим на Бинансе
                                            var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);

                                            var messageError = "";
                                            if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                            {
                                                OnMessageDebugEvent($"Открытие позиции: УСПЕШНО. Ключ: {nameKey}");
                                                logService.Write($"Открытие позиции: УСПЕШНО. Ключ: {nameKey}");
                                                // запрашиваем сделки с биржи
                                                if (!RequestedTrades(publicKey, secretKey, pair, long.Parse(orderResponse.OrderId.Trim()), (decimal)stopOrder.Amount))
                                                {
                                                    OnMessageDebugEvent("Не удалось получить последние сделки по счету.");
                                                    logService.Write("RequestedTrades: FAILED");
                                                    EndTask();
                                                    return;
                                                }
                                                // считаем среднюю цену позы
                                                var getAvgResult = GetAvgPricePosition(tradeRepository, tradeConfiguration.ActivationTime, publicKey, pair, !stopOrder.IsBuyOperation);
                                                logService.Write($"getAvgResult.AvgPrice: {getAvgResult.AvgPrice} getAvgResult.SumAmount: {getAvgResult.SumAmount}");

                                                // снимаем все стопы-loss и профиты
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair, !stopOrder.IsBuyOperation);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                if (stopPriceForTakeProfit <= 0)
                                                {
                                                    stopPriceForTakeProfit = RoundBaseAsset(getAvgResult.AvgPrice - (getAvgResult.AvgPrice * tradeConfiguration.Profit / 100), exchangeSettingsPair.BasePrecision);
                                                }
                                                logService.Write($"stopPriceForTakeProfit: {stopPriceForTakeProfit}");

                                                // выставляем профит
                                                var profitStop = new TakeProfitOrder()
                                                {
                                                    FK_PublicKey = publicKey,
                                                    Pair = stopOrder.Pair,
                                                    StopPrice = stopPriceForTakeProfit,
                                                    IndentExtremum = tradeConfiguration.IndentExtremum,
                                                    ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                                                    Amount = RoundLotSize(Math.Abs(getAvgResult.SumAmount), exchangeSettingsPair.LotSizeFilter.StepSize),
                                                    IsBuyOperation = false,
                                                    Active = true
                                                };
                                                //var profitStop = CreateTakeProfitOrder(publicKey, stopOrder.Pair, getAvgResult.AvgPrice, getAvgResult.SumAmount, true);
                                                takeProfitRepository.Create(profitStop);
                                                logService.Write("выставляем профит");
                                                logService.Write($"Key: {nameKey} Pair: {profitStop.Pair} StopPrice: {profitStop.StopPrice} IndentExtremum: {profitStop.IndentExtremum} ProtectiveSpread: {profitStop.ProtectiveSpread} Amount: {profitStop.Amount} IsBuyOperation: {profitStop.IsBuyOperation} Active: {profitStop.Active}");

                                                // выставляем лосс
                                                if (tradeConfiguration.Loss > 0)
                                                {
                                                    if (stopPriceForStopLoss <= 0)
                                                    {
                                                        stopPriceForStopLoss = RoundBaseAsset(getAvgResult.AvgPrice + (getAvgResult.AvgPrice * tradeConfiguration.Loss / 100), exchangeSettingsPair.BasePrecision);
                                                    }
                                                    logService.Write($"stopPriceForStopLoss: {stopPriceForStopLoss}");

                                                    var lossStop = new StopLimitOrder()
                                                    {
                                                        FK_PublicKey = publicKey,
                                                        Pair = stopOrder.Pair,
                                                        StopPrice = stopPriceForStopLoss,
                                                        Price = 0,
                                                        Amount = RoundLotSize(Math.Abs(getAvgResult.SumAmount), exchangeSettingsPair.LotSizeFilter.StepSize),
                                                        IsBuyOperation = false,
                                                        Active = true
                                                    };
                                                    //double stopPrice = GetStopLossPrice(getAvgResult.AvgPrice, tradeConfiguration.Loss, !stopOrder.IsBuyOperation);
                                                    //var lossStop = CreateStopLimitOrder(publicKey, stopOrder.Pair, stopPrice, RoundLotSize(Math.Abs(getAvgResult.SumAmount)), true);
                                                    stopLimitRepository.Create(lossStop);
                                                    logService.Write("выставляем лосс");
                                                    logService.Write($"Key: {nameKey} Pair: {lossStop.Pair} StopPrice: {lossStop.StopPrice} Price: {lossStop.Price} Amount: {lossStop.Amount} IsBuyOperation: {lossStop.IsBuyOperation} Active: {lossStop.Active}");
                                                }
                                            }
                                            else
                                            {
                                                OnMessageDebugEvent("Открытие позиции: ОШИБКА");
                                                logService.Write($"Открытие позиции: Ключ: {nameKey} ОШИБКА - {messageError}");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                //IsActiveAlgoritm = false;
                                                //EndTask();
                                                //return;

                                                apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                            }
                                        }
                                        else // сработал стоп-лосс
                                        {
                                            logService.Write("Сработал стоп-лосс");
                                            // выставляеим на Бинансе
                                            var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);

                                            var messageError = "";
                                            if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                            {
                                                OnMessageDebugEvent("Стоп-лосс на Бинансе: УСПЕШНО");
                                                logService.Write("Стоп-лосс на Бинансе: УСПЕШНО");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                                // начинаем заново
                                                //StartAlgoritm();
                                                //EndTask();
                                                //return;

                                                isExecutionAnyOrder = true;
                                            }
                                            else
                                            {
                                                OnMessageErrorEvent($"ВНИМАНИЕ! Стоп-лосс ошибка! Ключ: {nameKey}");
                                                logService.Write($"ВНИМАНИЕ! Стоп-лосс ошибка Ключ: {nameKey} Error: {messageError}");
                                                // все снимаем
                                                stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                                takeProfitRepository.DeactivationAllOrders(publicKey, pair);

                                                //IsActiveAlgoritm = false;
                                                //EndTask();
                                                //return;

                                                apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                            }
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
                                    OnMessageDebugEvent($"System error 3: {ex.Message}");
                                    logService.Write($"System error 3: {ex.Message}");
                                    throw ex;
                                }
                            }

                            try
                            {
                                // отслеживание тейк-профитов
                                logService.Write($"---- отслеживание тейк-профитов");
                                var takeProfits = takeProfitRepository.GetActive(pair).OrderByDescending(x => x.StopPrice);

                                logService.Write($"foreach (var order in takeProfits) I -----------------");
                                foreach (var order in takeProfits)
                                {
                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    logService.Write($"lastPrice: {lastPrice} stopOrder.StopPrice: {order.StopPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                                    if (lastPrice <= order.StopPrice && order.ExtremumPrice <= 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Initialize ExtremumPrice: {lastPrice}");
                                        logService.Write($"Initialize ExtremumPrice: {lastPrice}");
                                        takeProfitRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }
                                logService.Write($"end I -----------------");

                                takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderBy(x => x.ExtremumPrice);

                                logService.Write($"foreach (var order in takeProfits) II -----------------");
                                foreach (var order in takeProfits)
                                {
                                    var indentExtremum = ((lastPrice - order.ExtremumPrice) * 100) / order.ExtremumPrice;
                                    //OnMessageDebugEvent($"IE:{indentExtremum} * OIE:{order.IndentExtremum}");
                                    logService.Write($"indentExtremum: {indentExtremum} order.IndentExtremum: {order.IndentExtremum}");

                                    if (indentExtremum >= order.IndentExtremum)
                                    {
                                        // get secret key
                                        var publicKey = order.FK_PublicKey;
                                        var secretKey = apiKeypository.GetSecretKey(publicKey);
                                        var nameKey = apiKeypository.GetNameKey(publicKey);

                                        logService.Write($"Сработал тейк-профит: Key: {nameKey} Pair: {order.Pair} StopPrice: {order.StopPrice} IndentExtremum: {order.IndentExtremum} ProtectiveSpread: {order.ProtectiveSpread} Amount: {order.Amount} IsBuyOperation: {order.IsBuyOperation} Active: {order.Active}");

                                        // выставляеим на Бинансе
                                        var orderResponse = SendOrder(order.Pair, order.IsBuyOperation, order.Amount, publicKey, secretKey);

                                        var messageError = "";
                                        if (ProcessingErrorOrder(orderResponse, publicKey, out messageError))
                                        {
                                            OnMessageDebugEvent($"Тейк-профит: УСПЕШНО Ключ: {nameKey}");
                                            logService.Write($"Тейк-профит: УСПЕШНО Ключ: {nameKey}");
                                            // все снимаем
                                            stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все стоп-лимиты. {publicKey}");
                                            takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все тейк-профиты. {publicKey}");
                                            // начинаем заново
                                            //StartAlgoritm();
                                            //EndTask();
                                            //return;

                                            isExecutionAnyOrder = true;
                                        }
                                        else
                                        {
                                            OnMessageErrorEvent($"ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Ключ: {nameKey}");
                                            logService.Write($"ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Ключ: {nameKey} Error: {messageError}");

                                            // все снимаем
                                            stopLimitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все стоп-лимиты. {publicKey}");
                                            takeProfitRepository.DeactivationAllOrders(publicKey, pair);
                                            logService.Write($"Сняты все тейк-профиты. {publicKey}");

                                            //IsActiveAlgoritm = false;
                                            //EndTask();
                                            //return;

                                            apiKeypository.UpdateStatus(publicKey, false); // ставим по ключу статус ERROR
                                        }
                                    }

                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    logService.Write($"Update ExtremumPrice");
                                    logService.Write($"lastPrice: {lastPrice} order.ExtremumPrice: {order.ExtremumPrice}");
                                    if (lastPrice < order.ExtremumPrice && order.ExtremumPrice > 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Update ExtremumPrice: {lastPrice}");
                                        takeProfitRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }
                                logService.Write($"end II -----------------");
                                //----
                            }
                            catch (Exception ex)
                            {
                                // TODO: loging
                                OnMessageDebugEvent($"System error 4: {ex.Message}");
                                logService.Write($"System error 4: {ex.Message}");
                                throw ex;
                            }
                        }
                        if (isExecutionAnyOrder)
                        {
                            StartAlgoritm(tradeConfiguration); // начинаем заново
                        }
                        EndTask();
                    });
                }
            }
        }

        private void EndTask()
        {
            logService.Write($"***  end TASK ***", true);
        }

        private double GetAvgPriceOrders(List<StopLimitOrder> orders)
        {
            double result = 0;
            if (orders != null)
            {
                var sumMoney = orders.Sum(s => (decimal)s.StopPrice * (decimal)s.Amount);
                var sumAmount = orders.Sum(s => (decimal)s.Amount);
                result = (double)(sumMoney / sumAmount);
            }
            return result;
        }


        #region test takeprofit
        public void TakeProfitTraking(double _lastPrice)
        {
            //throw new NotImplementedException();
            // отслеживание тейк-профитов
            var takeProfits = repositoriesM.TakeProfitOrderRepository.GetActive("BTCUSDT").OrderBy(x => x.StopPrice);
            foreach (var order in takeProfits)
            {
                if (_lastPrice >= order.StopPrice && order.ExtremumPrice <= 0)
                {
                    // update ExtremumPrice
                    repositoriesM.TakeProfitOrderRepository.UpdateExtremumPrice(order.ID, _lastPrice);
                }
                else
                {
                    break;
                }
            }

            takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderByDescending(x => x.ExtremumPrice);
            foreach (var order in takeProfits)
            {
                // get secret key
                //var publicKey = order.FK_PublicKey;
                //var secretKey = repositoriesM.APIKeyRepository.GetSecretKey(publicKey);

                var indentExtremum = ((order.ExtremumPrice - _lastPrice) * 100) / order.ExtremumPrice;
                if (indentExtremum >= order.IndentExtremum)
                {
                    OnMessageErrorEvent("ВНИМАНИЕ! Тейк-профит испонился!");
                    //// выставляеим на Бинансе
                    //var orderResponse = SendOrder(order.Pair, order.IsBuyOperation, order.Amount, publicKey, secretKey);
                    //if (!ProcessingErrorOrder(orderResponse, publicKey))
                    //{
                    //    OnMessageErrorEvent("ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Проверьте баланс счета.");
                    //}
                    // все снимаем
                    repositoriesM.StopLimitOrderRepository.DeactivationAllOrders("ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO");
                    repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders("ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO");
                    //// начинаем заново
                    //StartAlgoritm();
                }

                if (_lastPrice > order.ExtremumPrice && order.ExtremumPrice > 0)
                {
                    // update ExtremumPrice
                    repositoriesM.TakeProfitOrderRepository.UpdateExtremumPrice(order.ID, _lastPrice);
                }
            }
        }
        #endregion

        private bool RequestedTrades(string publicKey, string secretKey, string pair, long lastOrderId, decimal lastOrderQty) // работает только при наличии сделок. Запускать только после исполнения хотя бы одной заявки.
        {
            logService.Write($"++ RequestedTrades: START", true);
            logService.Write($"Input values: publicKey={publicKey} secretKey=*** pair={pair} lastOrderId={lastOrderId} lastOrderQty={lastOrderQty}");
            const int countRequest = 1000;
            const int sleepMillisecond = 2000;
            for (int i = 1; i <= countRequest; i++)
            {
                OnMessageDebugEvent($"Попытка получить сделки № {i}");
                var trades = repositoriesM.TradeAccountInfo.RequestedTrades(publicKey, secretKey, pair);
                if(trades != null)
                {
                    if (trades.Count > 0)
                    {
                        var tradesLastOrder = trades.Where(x => x.orderId == lastOrderId).ToList();

                        var sumLastTrades = 0m;
                        if (tradesLastOrder != null)
                        {
                            sumLastTrades = tradesLastOrder.Sum(t => (decimal)t.qty);
                        }

                        logService.Write($"\t\t\tsumLastTrades: {sumLastTrades} lastOrderQty: {lastOrderQty}");
                        if (sumLastTrades == lastOrderQty)
                        {
                            logService.Write($"++ RequestedTrades: SUCCESS", true);
                            OnMessageDebugEvent($"Сделки успешно получены.");
                            return true;
                        }
                    }
                    else
                    {
                        logService.Write($"\t\ttrades.Count <= 0");
                    }
                }
                else
                {
                    logService.Write($"\ttrades != null");
                }
                Thread.Sleep(sleepMillisecond);
            }
            logService.Write($"++ RequestedTrades: countRequest = {countRequest}. Получить сделки с биржи не удалось");
            logService.Write($"++ RequestedTrades: FAILED", true);
            return false;
        }

        private OrderResponse SendOrder(string pair, bool isBuyer, double amount, string publicKey, string secretKey)
        {
            var orderSender = new OrderSender();
            var parametrs = orderSender.GetTransacParam(pair, isBuyer, amount);

            if(logService != null)
            {
                logService.Write($"----------------- SendOrder -----------------");
                logService.Write($"parametrs: {parametrs}");
                logService.Write($"---------------------------------------------");
            }
            
            var orderResponse = orderSender.Order(parametrs, publicKey, secretKey);
            return orderResponse;
        }

        private double GetStopPriceFirstOrder(bool isBuyer, double lastPrice, double orderIndent)
        {
            if (isBuyer)
            {
                return lastPrice - (lastPrice * orderIndent / 100);
            }
            else
            {
                return lastPrice + (lastPrice * orderIndent / 100);
            }
        }

        private double GetStopPriceNextOrder(double stopPricePreviosOrder, double firstStep, double orderStepPlus, bool isBuyer)
        {
            if (isBuyer)
            {
                return stopPricePreviosOrder - ((stopPricePreviosOrder * firstStep / 100) + (stopPricePreviosOrder * orderStepPlus / 100));
            }
            else
            {
                return stopPricePreviosOrder + ((stopPricePreviosOrder * firstStep / 100) + (stopPricePreviosOrder * orderStepPlus / 100));
            }
        }

        private double GetAmountFirstOrder(double orderDeposit, double lastPrice)
        {
            return orderDeposit / lastPrice;
        }

        private double GetAmountNextOrder(double amountPreviosOrder, double martingale)
        {
            return amountPreviosOrder + (amountPreviosOrder * martingale / 100);
        }

        private StopLimitOrder CreateStopLimitOrder(string key, string pair, double price, double amount, bool isBuyer)
        {
            return new StopLimitOrder()
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

        //private TakeProfitOrder CreateTakeProfitOrder(string key, string pair, double price, double amount, bool isBuyer)
        //{
        //    if (!isBuyer)
        //    {
        //        return new TakeProfitOrder()
        //        {
        //            FK_PublicKey = key,
        //            Pair = pair,
        //            StopPrice = RoundAsset(price + (price * tradeConfiguration.Profit / 100)),
        //            IndentExtremum = tradeConfiguration.IndentExtremum,
        //            ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
        //            Amount = RoundLotSize(Math.Abs(amount)),
        //            IsBuyOperation = isBuyer,
        //            Active = true
        //        };
        //    }
        //    else
        //    {
        //        return new TakeProfitOrder()
        //        {
        //            FK_PublicKey = key,
        //            Pair = pair,
        //            StopPrice = RoundAsset(price - (price * tradeConfiguration.Profit / 100)),
        //            IndentExtremum = tradeConfiguration.IndentExtremum,
        //            ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
        //            Amount = RoundLotSize(Math.Abs(amount)),
        //            IsBuyOperation = isBuyer,
        //            Active = true
        //        };
        //    }
        //}

        private double GetOrderLimit(double balance, double orderDeposit)
        {
            return balance * orderDeposit / 100;
        }
        private double GetAllowedBalance(double freeBalance, double depositLimit)
        {
            return freeBalance * depositLimit / 100;
        }

        private Balance GetBalanceBaseAsset(BalanceRepository balanceRepository, string publicKey, string baseAsset)
        {
            return balanceRepository.Get(publicKey, baseAsset);
        }

        private Balance GetBalanceQuoteAsset(BalanceRepository balanceRepository, string publicKey, string quoteAsset)
        {
            return balanceRepository.Get(publicKey, quoteAsset);
        }

        private double RoundBaseAsset(double value, int basePrecision)
        {
            return Math.Round(value, basePrecision);
        }
        private double RoundQuoteAsset(double value, int quotePrecision)
        {
            return Math.Round(value, quotePrecision);
        }

        private double RoundLotSize(double value, decimal stepSize)
        {
            decimal size = (decimal)value;
            int sizeInt = (int)(size / stepSize);
            decimal roundSize = sizeInt * stepSize;
            return (double)roundSize;
        }

        private ExchangeSettingsPair GetExchangeInfo(string pair)
        {
            var exchangeSettingsPair = new ExchangeSettingsPair();
            try
            {
                var jsonString = repositoriesM.ExchangeInfo.Info;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    if (symbol.symbol == pair)
                    {
                        exchangeSettingsPair.QuotePrecision = symbol.quotePrecision;
                        exchangeSettingsPair.BasePrecision = symbol.baseAssetPrecision;
                        foreach (var filter in symbol.filters)
                        {
                            if(filter.filterType == "LOT_SIZE")
                            {
                                decimal stepSize;
                                decimal.TryParse(filter.stepSize.ToString(), NumberStyles.Number, new CultureInfo("en-US"), out stepSize);
                                exchangeSettingsPair.LotSizeFilter.StepSize = stepSize;
                            }
                        }
                        break;
                    }
                }
                return exchangeSettingsPair;
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

        /* isBuyer - должно имееть противоположное значение по отношению к операции стоп-ордера (stopOrder.IsBuyOperation)
         * - для стоп-ордеров на открытие лонгов isBuyer должен быть false, т.к. нужно найти все сделки Купля с момента последней Продажи,
         * состоявшейся после применения конфига
         * - для стоп-ордеров на открытие шортов - наоборот
        */
        private AvgPricePositionResult GetAvgPricePosition(TradeRepository tradeRepository, long configActivationTime, string publicKey, string pair, bool isBuyer)
        {
            var result = new AvgPricePositionResult();
            var timeLastSell = tradeRepository.GetTimeLastTrade(publicKey, pair, isBuyer, configActivationTime);

            OnMessageDebugEvent($"timeLastSell: {timeLastSell}");
            logService.Write($"timeLastSell: {timeLastSell}");

            var trades = tradeRepository.Get(publicKey, pair, timeLastSell > 0 ? timeLastSell : configActivationTime, !isBuyer).ToList();
            if(trades != null)
            {
                var sumMoney = trades.Sum(x => (decimal)x.QuoteQty);
                var sumAmount = 0.0m;
                if (!isBuyer)
                {
                    sumAmount = trades.Sum(x => (decimal)x.Qty - (decimal)x.Commission);
                }
                else
                {
                    sumAmount = trades.Sum(x => (decimal)x.Qty);
                }
                var sumAmountWithoutCommision = trades.Sum(x => (decimal)x.Qty);
                var avgPrice = 0.0M;
                if (sumAmount != 0)
                {
                    avgPrice = sumMoney / sumAmountWithoutCommision;
                    result.SumAmount = (double)sumAmount;
                    result.AvgPrice = (double)avgPrice;
                }

                foreach (var trade in trades)
                {
                    logService.Write($"trade.ID: {trade.ID} trade.FK_PublicKey: {trade.FK_PublicKey.Substring(0,5)} trade.OrderID: {trade.OrderID} trade.Commission: {trade.Commission} trade.IsBuyer: {trade.IsBuyer} trade.Price: {trade.Price} trade.Qty: {trade.Qty} trade.Symbol: {trade.Symbol} trade.Time: {trade.Time} trade.TradeID: {trade.TradeID}");
                }

                logService.Write($"sumMoney: {sumMoney}");
                logService.Write($"sumAmount: {sumAmount}");
                logService.Write($"avgPrice: {avgPrice}");
            }
            return result;
        }

        private bool ProcessingErrorOrder(OrderResponse orderResponse, string publicKey, out string messageError)
        {
            messageError = "";
            if (orderResponse != null)
            {
                if (!string.IsNullOrWhiteSpace(orderResponse.Msg))
                {
                    messageError = $"Ошибка ({orderResponse.Msg}) при выставлении ордера по счету: {publicKey}";
                    OnMessageErrorEvent($"Ошибка ({orderResponse.Msg}) при выставлении ордера по счету: {publicKey}");
                    return false;
                }
            }
            else
            {
                messageError = $"Неизвестная ошибка при выставлении ордера по счету: {publicKey}.orderResponse = null";
                OnMessageErrorEvent($"Неизвестная ошибка при выставлении ордера по счету: {publicKey}.orderResponse = null");
                return false;
            }
            return true;
        }

        protected virtual void OnMessageErrorEvent(string e)
        {
            MessageErrorEvent(this, e);
        }

        protected virtual void OnMessageDebugEvent(string e)
        {
            MessageDebugEvent(this, e);
        }
    }

    public class RepositoriesM
    {
        public TradeConfigRepository TradeConfigRepository { get; set; }
        public APIKeyRepository APIKeyRepository { get; set; }
        public BalanceRepository BalanceRepository { get; set; }
        public StopLimitOrderRepository StopLimitOrderRepository { get; set; }
        public TakeProfitOrderRepository TakeProfitOrderRepository { get; set; }
        public TradeRepository TradeRepository { get; set; }
        public CurrentTrades CurrentTrades { get; set; }
        public ExchangeInfo ExchangeInfo { get; set; }
        public TradeAccountInfo TradeAccountInfo { get; set; }
    }
}
