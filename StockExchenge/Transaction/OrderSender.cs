using Services;
using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StockExchenge.Transaction
{
    public class OrderSender
    {
        public string GetTransacParamMarket(string pair, bool isBuy, double amount)
        {
            var operation = "";
            if (isBuy)
            {
                operation = "BUY";
            }
            else
            {
                operation = "SELL";
            }
            var amountStr = amount.ToString(CultureInfo.GetCultureInfo("en-US"));
            return $"symbol={pair}&side={operation}&quantity={amountStr}";
        }

        public string GetTransacParamLimit(string pair, bool isBuy, double amount, double price)
        {
            var operation = "";
            if (isBuy)
            {
                operation = "BUY";
            }
            else
            {
                operation = "SELL";
            }
            var amountStr = amount.ToString(CultureInfo.GetCultureInfo("en-US"));
            return $"symbol={pair}&side={operation}&quantity={amountStr}&price={price.ToString().Replace(",", ".")}";
        }

        public OrderResponse OrderMarket(string paramOrd, string publicKey, string secretKey)
        {
            string correctParam = paramOrd.Replace("_", "");

            var privateApi = new SecretKeyRequiredRequester();
            string response = string.Empty;

            Regex my_reg = new Regex(@"\D");
            string serverTime = string.Empty;

            try
            {
                serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
            }
            catch (Exception ex)
            {
                // TODO: loging
            }

            string url_order = "https://api.binance.com/api/v3/order?" + correctParam + "&type=MARKET&recvWindow=5000&timestamp=" + serverTime;
            string api_parametrs = correctParam + "&type=MARKET&recvWindow=5000&timestamp=" + serverTime;

            try
            {
                response = privateApi.GetWebRequest(url_order, api_parametrs, publicKey, secretKey, "POST");
            }
            catch (Exception ex)
            {
                // TODO: ВЫВЕСТИ НА МОРДУ
            }

            return JConverter.JsonConver<OrderResponse>(response);
        }

        public OrderResponse OrderLimit(string paramOrd, string publicKey, string secretKey)
        {
            string correctParam = paramOrd.Replace("_", "");

            var privateApi = new SecretKeyRequiredRequester();
            string response = string.Empty;

            Regex my_reg = new Regex(@"\D");
            string serverTime = string.Empty;

            try
            {
                serverTime = my_reg.Replace(ServiceRequests.ServerTime(), "");
            }
            catch (Exception ex)
            {
                // TODO: loging
            }

            string url_order = "https://api.binance.com/api/v3/order?" + correctParam + "&type=LIMIT&recvWindow=5000&timeInForce=GTC&timestamp=" + serverTime;
            string api_parametrs = correctParam + "&type=LIMIT&recvWindow=5000&timeInForce=GTC&timestamp=" + serverTime;

            try
            {
                response = privateApi.GetWebRequest(url_order, api_parametrs, publicKey, secretKey, "POST");
            }
            catch (Exception ex)
            {
                // TODO: ВЫВЕСТИ НА МОРДУ
            }

            return JConverter.JsonConver<OrderResponse>(response);
        }
    }
}
