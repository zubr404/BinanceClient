using DataBaseWork.Models;
using DataBaseWork.Repositories;
using Services;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StockExchenge.TradeAccount
{
    /// <summary>
    /// Информация о сделках по счету
    /// </summary>
    public class TradeAccountInfo
    {

        readonly APIKeyRepository keyRepo;
        readonly TradeConfigRepository configRepository;
        readonly TradeRepository tradeRepository;
        private IEnumerable<APIKey> keys;
        private IEnumerable<TradeConfiguration> configurations;

        public TradeAccountInfo(APIKeyRepository keyRepo, TradeConfigRepository configRepository, TradeRepository tradeRepository)
        {
            this.keyRepo = keyRepo;
            this.configRepository = configRepository;
            this.tradeRepository = tradeRepository;
        }

        public void RequestedTrades()
        {
            keys = keyRepo.Get()?.ToList();
            configurations = configRepository.GetActive()?.ToList();
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    if (configurations != null)
                    {
                        foreach (var configuration in configurations)
                        {
                            RequestedTrades(key.PublicKey, key.SecretKey, configuration);
                        }
                    }
                }
            }
        }

        public void RequestedTrades(string publicKey, string secretKey, TradeConfiguration configuration)
        {
            var trades = TradesRequest(publicKey, secretKey, $"{configuration.MainCoin}{configuration.AltCoin}");
            if (trades != null)
            {
                foreach (var trade in trades)
                {
                    tradeRepository.Create(new DataBaseWork.Models.Trade()
                    {
                        FK_PublicKey = publicKey,
                        Symbol = trade.symbol,
                        TradeID = trade.id,
                        OrderID = trade.orderId,
                        OrderListID = trade.orderListId,
                        Price = trade.price,
                        Qty = trade.qty,
                        QuoteQty = trade.quoteQty,
                        Commission = trade.commission,
                        CommissionAsset = trade.commissionAsset,
                        Time = trade.time,
                        IsBuyer = trade.isBuyer,
                        IsMaker = trade.isMaker,
                        IsBestMatch = trade.isBestMatch
                    });
                }
            }
        }

        private List<Trade> TradesRequest(string key, string secret, string pair)
        {
            SecretKeyRequiredRequester privateApi = new SecretKeyRequiredRequester();
            string response = string.Empty;
            Regex my_reg = new Regex(@"\D");
            string serverTime = string.Empty;
            try
            {
                serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
            }
            catch (Exception ex)
            {
                // TODO loging
            }
            try
            {
                response = privateApi.GetWebRequest($"{Resources.DOMAIN_V3}myTrades?symbol={pair}&recvWindow=5000&timestamp={serverTime}", $"symbol={pair}&recvWindow=5000&timestamp={serverTime}", key, secret, "GET");
            }
            catch (Exception ex)
            {
                // TODO loging
            }
            return JConverter.JsonConver<List<Trade>>(response);
        }
    }
}
