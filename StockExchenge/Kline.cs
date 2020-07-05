using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace StockExchenge
{
    public class Kline
    {
        public WebSocket WebSocket { get; private set; }

        public void SocketOpen(string pair, string interval)
        {
            WebSocket = new WebSocket($"{Resources.SOCKET}{pair.ToLower()}@kline_{interval}");
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnClose += WebSocket_OnClose;
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.Connect();
        }

        // test
        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("Socket open");
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("Socket close");
        }

        public void Disconnect()
        {
            WebSocket.Close();
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            //Console.WriteLine(e.Data);
            if (e.Data.Contains("ping"))
            {
                Console.WriteLine(e.Data);
            }
            Console.WriteLine(DateTime.Now);
        }
        //----- end yesy
    }
}
