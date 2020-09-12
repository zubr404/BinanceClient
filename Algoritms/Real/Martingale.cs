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

namespace Algoritms.Real
{
    public class Martingale
    {
        const string LONG_STRATEGY = "Long";
        const string SHORT_STRATEGY = "Short";

        readonly RepositoriesM repositoriesM;
        public bool IsActiveAlgoritm { get; set; }

        private TradeConfiguration tradeConfiguration;
        private List<APIKey> apiKeys;

        private CurrentPair currentPair;
        private double lastPrice;
        private int basePrecision;
        private int quotePrecision;

        public event EventHandler<string> MessageErrorEvent;
        public event EventHandler<string> MessageDebugEvent;

        private bool isReload;

        public Martingale(RepositoriesM repositoriesM)
        {
            this.repositoriesM = repositoriesM;
            this.repositoriesM.CurrentTrades.LastPriceEvent += CurrentTrades_LastPriceEvent;
            MessageErrorEvent = delegate { };
            MessageDebugEvent = delegate { };
        }

        public void StopAlgoritm()
        {
            IsActiveAlgoritm = false;
            // снимаем все стопы и профиты
            repositoriesM.StopLimitOrderRepository.DeactivationAllOrders();
            repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders();
        }

        public void StartAlgoritm()
        {
            isReload = true;
            var orders = new List<StopLimitOrder>();
            tradeConfiguration = repositoriesM.TradeConfigRepository.GetLast();
            SetCurrentPair();
            GetExchangeInfo();
            apiKeys = repositoriesM.APIKeyRepository.Get().ToList();

            var lastPriceMarketTrade = new LastPriceMarketTrade();
            var priceResponse = lastPriceMarketTrade.GetInfo(currentPair.Pair);
            if(priceResponse != null)
            {
                lastPrice = priceResponse.price;
            }
            else
            {
                OnMessageErrorEvent("Ошибка при получении цены последней сделки в блоке выставления начальной сетки. Алгоритм остановлен.");
                IsActiveAlgoritm = false;
                return;
            }

            var existsStopLimit = repositoriesM.StopLimitOrderRepository.ExistsActive();
            var existsTakeProfit = repositoriesM.TakeProfitOrderRepository.ExistsActive();

            if(!existsStopLimit && !existsTakeProfit)
            {
                foreach (var key in apiKeys)
                {
                    if (tradeConfiguration.Strategy == LONG_STRATEGY)
                    {
                        var balance = GetBalanceQuoteAsset(key.PublicKey);
                        var freeBalance = 0.0;
                        if (balance != null)
                        {
                            freeBalance = balance.Free;
                        }
                        if (freeBalance == 0)
                        {
                            continue;
                        }
                        var allowedBalance = RoundQuote(GetAllowedBalance(freeBalance));

                        var stopPricePreviosOrder = RoundQuote(lastPrice - (lastPrice * tradeConfiguration.OrderIndent / 100));
                        //var stopPricePreviosOrder = RoundQuote(10950.0 - (10950.0 * tradeConfiguration.OrderIndent / 100));

                        var amountPreviosOrder = tradeConfiguration.OrderDeposit;
                        orders.Add(new StopLimitOrder()
                        {
                            FK_PublicKey = key.PublicKey,
                            Pair = currentPair.Pair,
                            StopPrice = stopPricePreviosOrder,
                            Price = 0,
                            Amount = amountPreviosOrder,
                            IsBuyOperation = true,
                            Active = true
                        });
                        allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;

                        int counter = 0;
                        while (true)
                        {
                            stopPricePreviosOrder = RoundQuote(stopPricePreviosOrder - ((stopPricePreviosOrder * tradeConfiguration.FirstStep / 100) + (stopPricePreviosOrder * tradeConfiguration.OrderStepPlus / 100)));
                            amountPreviosOrder = RoundQuote(amountPreviosOrder + (amountPreviosOrder * tradeConfiguration.Martingale / 100));
                            allowedBalance -= amountPreviosOrder * stopPricePreviosOrder;
                            if (allowedBalance < 0)
                            {
                                break;
                            }
                            orders.Add(new StopLimitOrder()
                            {
                                FK_PublicKey = key.PublicKey,
                                Pair = currentPair.Pair,
                                StopPrice = stopPricePreviosOrder,
                                Price = 0,
                                Amount = amountPreviosOrder,
                                IsBuyOperation = true,
                                Active = true
                            });

                            if (counter > 999888)
                            {
                                IsActiveAlgoritm = false;
                                OnMessageErrorEvent("Ошибка при расчете сетки в блоке выставления начальной сетки. Алгоритм остановлен.");
                                return;
                            }
                            counter++;
                        }
                    }
                    else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
                    {

                    }
                }
                repositoriesM.StopLimitOrderRepository.Create(orders);
                OnMessageDebugEvent("Сетка ордеров создана.");
            }
            else
            {
                isReload = false;
                OnMessageDebugEvent("Сетка ордеров уже имеется.");
            }
            IsActiveAlgoritm = true;
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

        bool isDone = true;
        int countReturn = 0;
        private void CurrentTrades_LastPriceEvent(object sender, LastPriceEventArgs e)
        {
            if (!isDone) 
            {
                countReturn++;
                if(countReturn > 500)
                {
                    isDone = true;
                    countReturn = 0;
                    OnMessageDebugEvent("Есть вероятность ошибки!");
                }
                return;
            } // ждем окончания выполнения задачи, не останавливая поток данных о сделках

            isDone = false;
            //OnMessageDebugEvent("isDone = false;");
            Task.Run(() =>
            {
                if (currentPair != null)
                {
                    //OnMessageDebugEvent(currentPair.Pair);
                    if (e.Pair.ToUpper() == currentPair.Pair)
                    {
                        lastPrice = e.LastPrice;
                        //OnMessageDebugEvent(lastPrice.ToString());

                        if (IsActiveAlgoritm)
                        {
                            //OnMessageDebugEvent("ActiveAlgoritm");
                            if (tradeConfiguration.Strategy == LONG_STRATEGY)
                            {
                                //OnMessageDebugEvent("LONG_STRATEGY");
                                // отслеживание OrderReload
                                var maxPrice = repositoriesM.StopLimitOrderRepository.GetMaxStopPriceBuy(currentPair.Pair);
                                var indent = 0.0;
                                if(maxPrice > 0)
                                {
                                    indent = ((lastPrice - maxPrice) * 100) / maxPrice;
                                }
                                if (indent > tradeConfiguration.OrderReload && tradeConfiguration.OrderReload > 0 && isReload)
                                {
                                    OnMessageDebugEvent("Перемещение сетки OrderReload.");
                                    repositoriesM.StopLimitOrderRepository.DeactivationAllOrders();
                                    repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders();
                                    StartAlgoritm();
                                }

                                // отслеживание исполнения стопов
                                var stopOrders = repositoriesM.StopLimitOrderRepository.GetActive(currentPair.Pair).OrderByDescending(x => x.StopPrice);

                                foreach (var stopOrder in stopOrders)
                                {
                                    //OnMessageDebugEvent("отслеживание исполнения стопов");
                                    try
                                    {
                                        if (lastPrice <= stopOrder.StopPrice)
                                        {
                                            // get secret key
                                            var publicKey = stopOrder.FK_PublicKey;
                                            var secretKey = repositoriesM.APIKeyRepository.GetSecretKey(publicKey);

                                            if (stopOrder.IsBuyOperation)
                                            {
                                                isReload = false; // если есть иполнение запрещаем перестановку ордеров

                                                // обновляем стоп
                                                repositoriesM.StopLimitOrderRepository.DeaktivateOrder(stopOrder.ID);
                                                // выставляеим на Бинансе
                                                var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);

                                                if (ProcessingErrorOrder(orderResponse, publicKey))
                                                {
                                                    OnMessageDebugEvent("Открытие позы на Бинансе: УСПЕШНО");
                                                    // запрашиваем сделки с биржи
                                                    repositoriesM.TradeAccountInfo.RequestedTrades(publicKey, secretKey, tradeConfiguration);
                                                    // считаем среднюю цену позы
                                                    var getAvgResult = GetAvgPricePosition(publicKey, false);
                                                    // снимаем все стопы-loss и профиты
                                                    repositoriesM.StopLimitOrderRepository.DeactivationAllOrders(publicKey, false);
                                                    repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                                    // выставляем профит
                                                    repositoriesM.TakeProfitOrderRepository.Create(new TakeProfitOrder()
                                                    {
                                                        FK_PublicKey = publicKey,
                                                        Pair = stopOrder.Pair,
                                                        StopPrice = getAvgResult.AvgPrice + (getAvgResult.AvgPrice * tradeConfiguration.Profit / 100),
                                                        IndentExtremum = tradeConfiguration.IndentExtremum,
                                                        ProtectiveSpread = tradeConfiguration.ProtectiveSpread,
                                                        Amount = Math.Abs(getAvgResult.SumAmount),
                                                        IsBuyOperation = false,
                                                        Active = true
                                                    });
                                                    // выставляем лосс
                                                    repositoriesM.StopLimitOrderRepository.Create(new StopLimitOrder()
                                                    {
                                                        FK_PublicKey = publicKey,
                                                        Pair = stopOrder.Pair,
                                                        StopPrice = getAvgResult.AvgPrice - (getAvgResult.AvgPrice * tradeConfiguration.Loss / 100),
                                                        Price = 0,
                                                        Amount = getAvgResult.SumAmount,
                                                        IsBuyOperation = false,
                                                        Active = true
                                                    });
                                                }
                                                else
                                                {
                                                    OnMessageDebugEvent("Открытие позы на Бинансе: ОШИБКА");
                                                }
                                            }
                                            else // сработал стоп-лосс
                                            {
                                                // выставляеим на Бинансе
                                                var orderResponse = SendOrder(stopOrder.Pair, stopOrder.IsBuyOperation, stopOrder.Amount, publicKey, secretKey);
                                                if (ProcessingErrorOrder(orderResponse, publicKey))
                                                {
                                                    OnMessageDebugEvent("Стоп-лосс на Бинансе: УСПЕШНО");
                                                    // все снимаем
                                                    repositoriesM.StopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                                    repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                                    // начинаем заново
                                                    StartAlgoritm();
                                                }
                                                else
                                                {
                                                    OnMessageErrorEvent("ВНИМАНИЕ! Стоп-лосс испонился с ошибкой! Проверьте баланс счета.");
                                                    // все снимаем
                                                    repositoriesM.StopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                                    repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders(publicKey);
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
                                        OnMessageDebugEvent($"System error: {ex.Message}");
                                    }
                                }

                                // отслеживание тейк-профитов
                                var takeProfits = repositoriesM.TakeProfitOrderRepository.GetActive(currentPair.Pair).OrderBy(x => x.StopPrice);
                                foreach (var order in takeProfits)
                                {
                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    if (lastPrice >= order.StopPrice && order.ExtremumPrice <= 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Initialize ExtremumPrice: {lastPrice}");
                                        repositoriesM.TakeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }

                                takeProfits = takeProfits.Where(x => x.ExtremumPrice > 0).OrderByDescending(x => x.ExtremumPrice);
                                foreach (var order in takeProfits)
                                {
                                    // get secret key
                                    var publicKey = order.FK_PublicKey;
                                    var secretKey = repositoriesM.APIKeyRepository.GetSecretKey(publicKey);

                                    var indentExtremum = ((order.ExtremumPrice - lastPrice) * 100) / order.ExtremumPrice;
                                    //OnMessageDebugEvent($"IE:{indentExtremum} * OIE:{order.IndentExtremum}");
                                    if (indentExtremum >= order.IndentExtremum)
                                    {
                                        // выставляеим на Бинансе
                                        var orderResponse = SendOrder(order.Pair, order.IsBuyOperation, order.Amount, publicKey, secretKey);
                                        if (ProcessingErrorOrder(orderResponse, publicKey))
                                        {
                                            OnMessageDebugEvent("Тейк-профит на Бинансе: УСПЕШНО");
                                            // все снимаем
                                            repositoriesM.StopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                            repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                            // начинаем заново
                                            StartAlgoritm();
                                        }
                                        else
                                        {
                                            OnMessageErrorEvent("ВНИМАНИЕ! Тейк-профит испонился с ошибкой! Проверьте баланс счета.");
                                            // все снимаем
                                            repositoriesM.StopLimitOrderRepository.DeactivationAllOrders(publicKey);
                                            repositoriesM.TakeProfitOrderRepository.DeactivationAllOrders(publicKey);
                                        }
                                    }

                                    //OnMessageDebugEvent($"LP:{lastPrice} * SP:{order.StopPrice}");
                                    if (lastPrice > order.ExtremumPrice && order.ExtremumPrice > 0)
                                    {
                                        // update ExtremumPrice
                                        OnMessageDebugEvent($"Update ExtremumPrice: {lastPrice}");
                                        repositoriesM.TakeProfitOrderRepository.UpdateExtremumPrice(order.ID, lastPrice);
                                    }
                                }
                                //----
                            }
                            else if (tradeConfiguration.Strategy == SHORT_STRATEGY)
                            {

                            }
                        }
                    }
                }

                isDone = true;
                countReturn = 0;
                //OnMessageDebugEvent("isDone = true;");
            });
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

        private OrderResponse SendOrder(string pair, bool isBuyer, double amount, string publicKey, string secretKey)
        {
            var orderSender = new OrderSender();
            var parametrs = orderSender.GetTransacParam(pair, isBuyer, amount);
            var orderResponse = orderSender.Order(parametrs, publicKey, secretKey);
            return orderResponse;
        }

        private void SetCurrentPair()
        {
            currentPair = new CurrentPair(tradeConfiguration.MainCoin, tradeConfiguration.AltCoin);
        }

        private double GetOrderLimit(double balance)
        {
            return balance * tradeConfiguration.OrderDeposit / 100;
        }
        private double GetAllowedBalance(double freeBalance)
        {
            return freeBalance * tradeConfiguration.DepositLimit / 100;
        }

        private Balance GetBalanceBaseAsset(string publicKey)
        {
            return repositoriesM.BalanceRepository.Get(publicKey, currentPair.BaseAsset);
        }

        private Balance GetBalanceQuoteAsset(string publicKey)
        {
            return repositoriesM.BalanceRepository.Get(publicKey, currentPair.QuoteAsset);
        }

        private double RoundBase(double value)
        {
            return Math.Round(value, basePrecision);
        }
        private double RoundQuote(double value)
        {
            return Math.Round(value, quotePrecision);
        }

        private void GetExchangeInfo()
        {
            try
            {
                var jsonString = repositoriesM.ExchangeInfo.Info;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    if (symbol.symbol == currentPair.Pair)
                    {
                        quotePrecision = symbol.quotePrecision;
                        basePrecision = symbol.baseAssetPrecision;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: запись лога в БД
            }
        }

        struct AvgPricePositionResult
        {
            public double SumAmount { get; set; }
            public double AvgPrice { get; set; }
        }

        private AvgPricePositionResult GetAvgPricePosition(string publicKey, bool isBuyer)
        {
            var result = new AvgPricePositionResult();
            var timeLastSell = repositoriesM.TradeRepository.GetTimeLastTrade(publicKey, isBuyer, tradeConfiguration.ActivationTime);

            OnMessageDebugEvent($"timeLastSell: {timeLastSell}");

            var trades = repositoriesM.TradeRepository.Get(publicKey, timeLastSell > 0 ? timeLastSell : tradeConfiguration.ActivationTime, !isBuyer);
            if(trades != null)
            {
                var sumMoney = trades.Sum(x => x.QuoteQty);
                var sumAmount = trades.Sum(x => x.Qty - x.Commission);
                var avgPrice = 0.0;
                if (sumAmount != 0)
                {
                    avgPrice = sumMoney / sumAmount;
                    result.SumAmount = sumAmount;
                    result.AvgPrice = avgPrice;
                }
            }
            return result;
        }

        private bool ProcessingErrorOrder(OrderResponse orderResponse, string publicKey)
        {
            if (orderResponse != null)
            {
                if (!string.IsNullOrWhiteSpace(orderResponse.Msg))
                {
                    OnMessageErrorEvent($"Ошибка ({orderResponse.Msg}) при выставлении ордера по счету: {publicKey}");
                    return false;
                }
            }
            else
            {
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
