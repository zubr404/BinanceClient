using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using System.Globalization;

namespace TestApp.WebSocket
{
    public class Kline
    {
        WebSocketSharp.WebSocket webSocket;
        public void KlineWebSocket()
        {
            webSocket = new WebSocketSharp.WebSocket($"wss://stream.binance.com:9443/ws/ethbtc@kline_1m");
            webSocket.OnMessage += (sender, e) =>
            {
                string jsonLine = e.Data.Replace(",[]", "");
                Console.WriteLine(jsonLine);

            };
            webSocket.OnError += (sender, e) => Console.WriteLine(e.Message);
            webSocket.Connect();
        }
    }
}
