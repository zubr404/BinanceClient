using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace StockExchenge.RestApi
{
    public class SecretKeyRequiredRequester
    {
        public string GetWebRequest(string address, string api_parameters, string key, string secret, string methodMode)
        {
            string result = string.Empty;

            WebRequest webRequest = null;

            string signature = DigitalSignature(api_parameters, secret);
            string finalUrl = address + "&signature=" + signature;

            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create(finalUrl);
            }
            catch (System.Exception ex)
            {
                // TODO: loging
            }

            if (webRequest != null)
            {
                webRequest.Method = methodMode;
                webRequest.Timeout = 20000;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Headers.Add("X-MBX-APIKEY", key);

                WebResponse webResponse = null;

                try
                {
                    webResponse = webRequest.GetResponse();
                }
                catch (WebException wex)
                {
                    string strEr;
                    using (Stream stream = wex.Response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            strEr = reader.ReadToEnd();
                        }
                    }

                    return strEr;
                }

                using (System.IO.Stream s = webResponse.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }
            else
            {
                result = "webRequest = null";
                // TODO: loging
            }

            return result;
        }

        /// <summary>
        /// Электронная подпись
        /// </summary>
        /// <param name="api_parameters">Например method=getInfo&nonce=2</param>
        /// <param name="secret">секретный ключ</param>
        /// <returns></returns>
        private string DigitalSignature(string api_parameters, string secret)
        {
            var keyByte = Encoding.UTF8.GetBytes(secret);

            byte[] inputBytes = Encoding.UTF8.GetBytes(api_parameters);
            using (var hmac = new HMACSHA256(keyByte))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);

                StringBuilder hex1 = new StringBuilder(hashValue.Length * 2);
                foreach (byte b in hashValue)
                {
                    hex1.AppendFormat("{0:x2}", b);
                }
                return hex1.ToString();
            }
        }
    }
}
