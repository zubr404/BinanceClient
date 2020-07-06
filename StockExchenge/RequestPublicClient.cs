using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace StockExchenge
{
    class RequestPublicClient
    {
        public WebRequest WebRequestCreate(string uriString)
        {
            try
            {
                return WebRequest.Create(uriString);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public WebResponse GetWebResponse(WebRequest webRequest)
        {
            try
            {
                return webRequest.GetResponse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetResponseString(WebResponse webResponse)
        {
            try
            {
                using (StreamReader stream = new StreamReader(webResponse.GetResponseStream()))
                {
                    return stream.ReadLine();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
