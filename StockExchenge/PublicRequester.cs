using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace StockExchenge
{
    public class PublicRequester
    {
        // обобщенный запрос к публичным API
        public string RequestPublicApi(string uri)
        {
            try
            {
                RequestPublicClient webApiClient = new RequestPublicClient();

                WebRequest request = webApiClient.WebRequestCreate(uri);
                WebResponse response = webApiClient.GetWebResponse(request);
                string line1 = webApiClient.GetResponseString(response);

                return line1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
