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
        readonly Services.LogService logService;

        public AccountInfo(APIKeyRepository keyRepo, BalanceRepository balanceRepo)
        {
            this.keyRepo = keyRepo;
            this.balanceRepo = balanceRepo;
            logService = new LogService();
            logService.CreateLogFile("AccountInfo");
        }

        public void RequestedBalances()
        {
            logService.Write("***********RequestedBalances START***********", true);
            try
            {
                keys = keyRepo.Get()?.ToList();
                if (keys != null)
                {
                    logService.Write("\tRequestedBalances get keys successful.");
                    foreach (var key in keys)
                    {
                        logService.Write($"\tPublic key: {key.PublicKey}");
                        var account = Account(key.PublicKey, key.SecretKey);
                        if (account != null)
                        {
                            logService.Write("\tRequestedBalances get account successful.");
                            if (account.Balances != null)
                            {
                                logService.Write("\tRequestedBalances get Balances successful.");
                                foreach (var balance in account.Balances)
                                {
                                    SaveBalance(balance, key.PublicKey);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logService.Write($"RequestedBalances error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}", true);
            }
            logService.Write("***********RequestedBalances END***********", true);
        }

        private void SaveBalance(Balance balance, string publicKey)
        {
            try
            {
                if (balance.Free != 0 || balance.Free != 0)
                {
                    balanceRepo.Update(new DataBaseWork.Models.Balance()
                    {
                        FK_PublicKey = publicKey,
                        Asset = balance.Asset,
                        Free = balance.Free,
                        Locked = balance.Locked
                    });
                    logService.Write($"\t\tAsset: {balance.Asset} Free: {balance.Free} Locked: {balance.Locked}");
                }
            }
            catch (Exception ex)
            {
                logService.Write($"\t\tSaveBalance error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }
        }

        private Account Account(string key, string secret)
        {
            logService.Write("***********Account START***********", true);
            logService.Write($"\tPublic key: {key}");
            Account result = null;
            SecretKeyRequiredRequester privateApi = new SecretKeyRequiredRequester();
            string response = string.Empty;
            Regex my_reg = new Regex(@"\D");
            string serverTime = string.Empty;
            try
            {
                serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
                logService.Write($"\tAccount get serverTime successful: {serverTime}");
            }
            catch (Exception ex)
            {
                logService.Write($"\tAccount get serverTime error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }
            try
            {
                response = privateApi.GetWebRequest($"{Resources.DOMAIN_V3}account?recvWindow=5000&timestamp=" + serverTime, "recvWindow=5000&timestamp=" + serverTime, key, secret, "GET");
                logService.Write($"\tAccount {Resources.DOMAIN_V3}account... successful: response = {response.Substring(0, 500)}");
            }
            catch (Exception ex)
            {
                logService.Write($"\tAccount {Resources.DOMAIN_V3}account... error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }

            try
            {
                result = JConverter.JsonConver<Account>(response);
                logService.Write($"\tAccount JConverter successful.");
            }
            catch (Exception ex)
            {
                logService.Write($"\tAccount JConverter error: Exception: {ex.Message} Innerexception: {ex.InnerException?.Message}");
            }
            logService.Write("***********Account END***********", true);
            return result;
        }
    }
}
