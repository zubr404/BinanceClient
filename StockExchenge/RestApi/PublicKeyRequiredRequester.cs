using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace StockExchenge.RestApi
{
    public class PublicKeyRequiredRequester
    {
        // 33SB2WjAtgVzFjcSGLE4fuvxzBQD8sz475bmC29UI8WCwtOVmdKwzqu78zVD6pqx key ключ
        public HttpWebResponse Request(string url, string publicKey)
        {
            try
            {
                var reqGET = (HttpWebRequest)WebRequest.Create(url);
                reqGET.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                reqGET.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0";
                reqGET.ContentType = "application/x-www-form-urlencoded";
                reqGET.Headers.Add("X-MBX-APIKEY", publicKey);

                var response = (HttpWebResponse)reqGET.GetResponse();
                return response;
            }
            catch (WebException ex)
            {
                return (HttpWebResponse)ex.Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
