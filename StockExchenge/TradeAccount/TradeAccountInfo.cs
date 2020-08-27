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
        private IEnumerable<APIKey> keys;
        private IEnumerable<TradeConfiguration> configurations;

        public TradeAccountInfo(APIKeyRepository keyRepo, TradeConfigRepository configRepository)
        {
            this.keyRepo = keyRepo;
            this.configRepository = configRepository;
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
                            var trade = TradesRequest(key.PublicKey, key.SecretKey, $"{configuration.MainCoin}{configuration.AltCoin}");
                            if (trade != null)
                            {
                                // save in DB
                            }
                        }
                    }
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
