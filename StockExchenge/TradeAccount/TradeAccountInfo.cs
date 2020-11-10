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
        readonly Services.LogService logService;

        public TradeAccountInfo(APIKeyRepository keyRepo, TradeConfigRepository configRepository, TradeRepository tradeRepository)
        {
            this.keyRepo = keyRepo;
            this.configRepository = configRepository;
            this.tradeRepository = tradeRepository;
            logService = new LogService();
            logService.CreateLogFile("TradeAccountInfo");
        }

        public void RequestedTrades()
        {
            logService.Write("***********RequestedTrades START***********", true);
            try
            {
                keys = keyRepo.Get()?.ToList();
                configurations = configRepository.GetActive()?.ToList();
                if (keys != null)
                {
                    logService.Write("\tRequestedTrades get keys successful.");
                    foreach (var key in keys)
                    {
                        logService.Write($"\tPublic key: {key.PublicKey}");
                        if (configurations != null)
                        {
                            foreach (var configuration in configurations)
                            {
                                RequestedTrades(key.PublicKey, key.SecretKey, $"{configuration.MainCoin}{configuration.AltCoin}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logService.Write($"RequestedTrades error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}", true);
            }
            logService.Write("***********RequestedTrades END***********", true);
        }

        public List<Trade> RequestedTrades(string publicKey, string secretKey, string pair)
        {
            logService.Write("***********RequestedTrades(string publicKey,... START***********", true);
            try
            {
                //var pair = $"{configuration.MainCoin}{configuration.AltCoin}";
                var fromId = tradeRepository.GetMaxId(publicKey, pair);
                var trades = TradesRequest(publicKey, secretKey, pair, fromId);
                var tradesForSave = new List<DataBaseWork.Models.Trade>();
                if (trades != null)
                {
                    logService.Write("\tRequestedTrades(string publicKey,... get trades successful.");
                    foreach (var trade in trades)
                    {
                        tradesForSave.Add(new DataBaseWork.Models.Trade()
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
                    tradeRepository.Create(tradesForSave);
                }
                logService.Write("***********RequestedTrades(string publicKey,... END***********", true);
                return trades;
            }
            catch (Exception ex)
            {
                logService.Write($"RequestedTrades(string publicKey,... error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}", true);
                logService.Write("***********RequestedTrades(string publicKey,... END***********", true);
                return null;
            }
        }

        //public List<Trade> RequestedTrades(string publicKey, string secretKey, TradeConfiguration configuration)
        //{
        //    var trades = TradesRequest(publicKey, secretKey, $"{configuration.MainCoin}{configuration.AltCoin}");
        //    if (trades != null)
        //    {
        //        foreach (var trade in trades)
        //        {
        //            tradeRepository.Create(new DataBaseWork.Models.Trade()
        //            {
        //                FK_PublicKey = publicKey,
        //                Symbol = trade.symbol,
        //                TradeID = trade.id,
        //                OrderID = trade.orderId,
        //                OrderListID = trade.orderListId,
        //                Price = trade.price,
        //                Qty = trade.qty,
        //                QuoteQty = trade.quoteQty,
        //                Commission = trade.commission,
        //                CommissionAsset = trade.commissionAsset,
        //                Time = trade.time,
        //                IsBuyer = trade.isBuyer,
        //                IsMaker = trade.isMaker,
        //                IsBestMatch = trade.isBestMatch
        //            });
        //        }
        //    }
        //    return trades;
        //}

        private List<Trade> TradesRequest(string key, string secret, string pair, long fromId)
        {
            logService.Write("***********TradesRequest START***********", true);
            logService.Write($"\tTradesRequest Public key: {key}");
            logService.Write($"\tTradesRequest pair: {pair}");
            List<Trade> result = null;
            var privateApi = new SecretKeyRequiredRequester();
            var my_reg = new Regex(@"\D");
            var serverTime = "";
            var response = "";
            try
            {
                serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
                logService.Write($"\tTradesRequest get serverTime successful: {serverTime}");
            }
            catch (Exception ex)
            {
                logService.Write($"\tAccount get serverTime error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }
            try
            {
                var request = CreatRequest(pair, serverTime, out string requestParams, fromId);
                response = privateApi.GetWebRequest(request, requestParams, key, secret, "GET");
                logService.Write($"\tTradesRequest {Resources.DOMAIN_V3}myTrades... successful: response = {response}");
            }
            catch (Exception ex)
            {
                logService.Write($"\tTradesRequest {Resources.DOMAIN_V3}myTrades... error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }

            try
            {
                result = JConverter.JsonConver<List<Trade>>(response);
                logService.Write($"\tTradesRequest JConverter successful.");
            }
            catch (Exception ex)
            {
                logService.Write($"\tTradesRequest JConverter error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }

            if(result != null)
            {
                var tradeForRemove = result.FirstOrDefault(x => x.id == fromId);
                if(tradeForRemove != null)
                {
                    result.Remove(tradeForRemove);
                }
            }
            logService.Write("***********TradesRequest END***********", true);
            return result;
        }

        private string CreatRequest(string pair, string serverTime, out string rquestParams, long fromId = -1)
        {
            if(fromId > 0)
            {
                rquestParams = $"symbol={pair}&fromId={fromId}&recvWindow=5000&timestamp={serverTime}";
                return $"{Resources.DOMAIN_V3}myTrades?symbol={pair}&fromId={fromId}&recvWindow=5000&timestamp={serverTime}";
            }
            else
            {
                rquestParams = $"symbol={pair}&recvWindow=5000&timestamp={serverTime}";
                return $"{Resources.DOMAIN_V3}myTrades?symbol={pair}&recvWindow=5000&timestamp={serverTime}";
            }
        }
    }
}
