using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace StockExchenge
{
    public class Kline
    {
        public WebSocket WebSocket { get; private set; }
        private readonly PublicRequester publicRequester;

        public event EventHandler<KlineEventArgs> MessageEvent;

        public Kline()
        {
            MessageEvent = delegate { };
            publicRequester = new PublicRequester();
        }

        public void SocketOpen(string pair, string interval)
        {
            Disconnect();
            WebSocket = new WebSocket($"{Resources.SOCKET}{pair.ToLower()}@kline_{interval}");
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnClose += WebSocket_OnClose;
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.Connect();
        }

        protected virtual void OnMessageEvent(KlineEventArgs e)
        {
            MessageEvent(this, e);
        }

        public string GetHistory(string pair, string interval)
        {
            try
            {
                return publicRequester.RequestPublicApi($"{Resources.DOMAIN}klines?symbol={pair.ToUpper()}&interval={interval}&limit=150");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public void Disconnect()
        {
            if (WebSocket != null)
            {
                WebSocket.OnMessage -= WebSocket_OnMessage;
                WebSocket.OnError -= WebSocket_OnError;
                WebSocket.OnClose -= WebSocket_OnClose;
                WebSocket.OnOpen -= WebSocket_OnOpen;
                WebSocket.Close();
                WebSocket = null;
            }
        }

        #region// test
        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
           //Console.WriteLine("Socket open");
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            //Console.WriteLine("Socket close");
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            //Console.WriteLine(e.Message);
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            //Console.WriteLine(e.Data);
            //if (e.Data.Contains("ping"))
            //{
            //    Console.WriteLine(e.Data);
            //}
            //Console.WriteLine(DateTime.Now);
            OnMessageEvent(new KlineEventArgs(e.Data));
        }
        #endregion
    }

    public class KlineEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public KlineEventArgs(string message)
        {
            Message = message;
        }
    }
}
