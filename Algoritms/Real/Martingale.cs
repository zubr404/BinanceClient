using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using StockExchenge.MarketTrades;
using DataBaseWork.Models;
using System.Linq;
using Services;
using BinanceClient;

namespace Algoritms.Real
{
    public class Martingale
    {
        // загружаем конфиги
        // загружаем ключи по счетам
        // загружаем балансы
        // получаем последние цены по парам
        // расчет используемого депозита
        // расчет депозита ордера
        // расчет отступа от рынка
        // ...

        // при изменении конфига все активные ордера должны быть удалены.

        // проверить наличие ордеров в БД
        // если ордеров нет: сохранить оредра в БД
        // если ордера есть: работаем с ними

        // отслеживаем последнюю цену сделки и исполняем ордера
        // при исполнении открывающего ордера расчитываем среднюю цену позиции (это можно сделать, выбрав сделки с операцией = настройки Strategy, которые шли после последней сделки с операцией != настройки Strategy)

        // если исполнился лосс или профит все начинаем заново

        readonly TradeConfigRepository tradeConfigRepository;
        readonly APIKeyRepository apiKeyRepository;
        readonly BalanceRepository balanceRepository;

        public bool IsActiveAlgoritm { get; set; }

        private TradeConfiguration tradeConfiguration;
        private IEnumerable<APIKey> apiKeys;

        private string currentPair;
        private double lastPrice;
        private int quotePrecision;

        public Martingale(TradeConfigRepository tradeConfigRepository, APIKeyRepository apiKeyRepository, BalanceRepository balanceRepository, CurrentTrades currentTrades)
        {
            this.tradeConfigRepository = tradeConfigRepository;
            this.apiKeyRepository = apiKeyRepository;
            this.balanceRepository = balanceRepository;
            currentTrades.LastPriceEvent += CurrentTrades_LastPriceEvent;
        }

        public void Start()
        {
            tradeConfiguration = tradeConfigRepository.GetLast();
            SetCurrentPair();
            GetExchangeInfo();
            apiKeys = apiKeyRepository.Get();

            foreach (var key in apiKeys)
            {
                var freeQuoteAsset = PrimitiveConverter.ToDouble(GetBalanceCuoteAsset(key.PublicKey)?.Free);
                var allowedBalance = GetAllowedBalance(freeQuoteAsset);
                
            }
            

            IsActiveAlgoritm = true;
        }

        private void CurrentTrades_LastPriceEvent(object sender, LastPriceEventArgs e)
        {
            if(e.Pair.ToUpper() == currentPair)
            {
                lastPrice = e.LastPrice;

                if (IsActiveAlgoritm)
                {

                }
            }
        }

        public double GetAllowedBalance(double freeQuoteAsset)
        {
            var result = freeQuoteAsset * tradeConfiguration.DepositLimit / 100;
            return Round(result);
        }

        private Balance GetBalanceCuoteAsset(string publicKey)
        {
            var balance = balanceRepository.Get(publicKey);
            return balance;
        }

        private void SetCurrentPair()
        {
            currentPair = $"{tradeConfiguration.MainCoin}{tradeConfiguration.AltCoin}".ToUpper();
        }

        private double Round(double value)
        {
            return Math.Round(value, quotePrecision);
        }

        private void GetExchangeInfo()
        {
            try
            {
                var jsonString = MainWindow.ExchangeInfo.Info;
                dynamic entity = JConverter.JsonConvertDynamic(jsonString);

                foreach (var symbol in entity.symbols)
                {
                    if (symbol.symbol == currentPair)
                    {
                        quotePrecision = symbol.quotePrecision;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: запись лога в БД
            }
        }
    }
}
