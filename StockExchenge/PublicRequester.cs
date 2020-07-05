using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge
{
    public class PublicRequester
    {
        // обобщенный запрос к публичным API
        public T RequestPublicApi<T>(string uri) where T : class
        {
            throw new NotImplementedException();
            /*RequestPublicClient webApiClient = new RequestPublicClient();

            WebRequest request = webApiClient.WebRequestCreate(uri);
            WebResponse response = webApiClient.GetWebResponse(request);
            string line1 = webApiClient.GetResponseString(response);

            return new JConverter().JsonConver<T>(line1);*/
        }
    }
}
