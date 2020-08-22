using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace StockExchenge.RestApi
{
    public class ServiceRequests
    {
        public static string ServerTime()
        {
            WebRequest webRequest = WebRequest.Create("https://api.binance.com/api/v1/time");
            WebResponse webResponse = webRequest.GetResponse();
            using (Stream stream = webResponse.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
