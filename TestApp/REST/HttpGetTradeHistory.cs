using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.REST
{
    class HttpGetTradeHistory
    {
        private int countRequest = 0;
        public async Task<int> Rquest1()
        {
            try
            {
                HttpWebRequest reqGET = (HttpWebRequest)WebRequest.Create(@"https://api.binance.com/api/v3/historicalTrades?symbol=BTCUSDT&fromId=1&limit=10");
                reqGET.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                reqGET.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0";
                reqGET.ContentType = "application/x-www-form-urlencoded";
                reqGET.Headers.Add("X-MBX-APIKEY", "33SB2WjAtgVzFjcSGLE4fuvxzBQD8sz475bmC29UI8WCwtOVmdKwzqu78zVD6pqx");


                HttpWebResponse response = (HttpWebResponse) await reqGET.GetResponseAsync();
                var status = (int)response.StatusCode;
                countRequest++;
                Console.Clear();
                Console.WriteLine($"{status} count: {countRequest}");

                WebHeaderCollection headers = response.Headers;
                for (int i = 0; i < headers.Count; i++)
                {
                    //Console.WriteLine("{0}: {1}", headers.GetKey(i), headers[i]);
                }


                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string s = sr.ReadToEnd();
                var trades = JConverter.JsonConver<List<StockExchenge.MarketTradesHistory.Trade>>(s);
                //Console.WriteLine(s);


                return status;
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp != null)
                {
                    var status = (int)resp.StatusCode;
                    Console.WriteLine(status);
                    return status;
                }
                else
                {
                    Console.WriteLine(ex.Message);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(-2);
                return -2;
            }
        }
    }
}
