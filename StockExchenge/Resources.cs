using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge
{
    public class Resources
    {
        // keys TODO: получать из БД
        public const string PUBLIC_KEY = "33SB2WjAtgVzFjcSGLE4fuvxzBQD8sz475bmC29UI8WCwtOVmdKwzqu78zVD6pqx";
        public const string SECRET_KEY = "eBDfUSGKluUOunoC3vBZTHHUIZNO24gjujJMTBZTO7cijVXUkC69GeBUtyVdX9lh";
        // web socket
        public const string SOCKET = "wss://stream.binance.com:9443/ws/";

        // rest api
        public const string DOMAIN_V1 = "https://api.binance.com/api/v1/";
        public const string DOMAIN_V3 = "https://api.binance.com/api/v3/";
    }
}
