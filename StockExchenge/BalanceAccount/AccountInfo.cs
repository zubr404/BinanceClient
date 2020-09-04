using DataBaseWork.Models;
using DataBaseWork.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StockExchenge.BalanceAccount
{
    /// <summary>
    /// Информация по счету, включая, Все балансы по счету
    /// </summary>
    public class AccountInfo
    {
        readonly APIKeyRepository keyRepo;
        readonly BalanceRepository balanceRepo;
        private IEnumerable<APIKey> keys;

        public AccountInfo(APIKeyRepository keyRepo, BalanceRepository balanceRepo)
        {
            this.keyRepo = keyRepo;
            this.balanceRepo = balanceRepo;
        }

        public void RequestedBalances()
        {
            keys = keyRepo.Get()?.ToList();
            if(keys != null)
            {
                foreach (var key in keys)
                {
                    var account = Account(key.PublicKey, key.SecretKey);
                    if (account != null)
                    {
                        if (account.Balances != null)
                        {
                            foreach (var balance in account.Balances)
                            {
                                SaveBalance(balance, key.PublicKey);
                            }
                        }
                    }
                }
            }
        }

        private void SaveBalance(Balance balance, string publicKey)
        {
            if(balance.Free != 0 || balance.Free != 0)
            {
                balanceRepo.Update(new DataBaseWork.Models.Balance()
                {
                    FK_PublicKey = publicKey,
                    Asset = balance.Asset,
                    Free = balance.Free,
                    Locked = balance.Locked
                });
            }
        }

        private Account Account(string key, string secret)
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
                response = privateApi.GetWebRequest($"{Resources.DOMAIN_V3}account?recvWindow=5000&timestamp=" + serverTime, "recvWindow=5000&timestamp=" + serverTime, key, secret, "GET");
            }
            catch (Exception ex)
            {
                // TODO loging
            }
            return JConverter.JsonConver<Account>(response);
        }
    }
}
