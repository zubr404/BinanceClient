using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using WebSocketSharp;

namespace StockExchenge
{
    public class Kline
    {
        public WebSocket WebSocket { get; private set; }
        private readonly PublicRequester publicRequester;

        public event EventHandler<KlineEventArgs> MessageEvent;
        public event EventHandler<string> ConnectStateEvent;

        string pair;
        string interval;
        private Timer timer;

        public Kline(string pair, string interval)
        {
            this.pair = pair;
            this.interval = interval;
            MessageEvent = delegate { };
            ConnectStateEvent = delegate { };
            publicRequester = new PublicRequester();
            timer = new Timer(30000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (WebSocket != null)
            {
                try
                {
                    if (WebSocket.Ping())
                    {
                        OnConnectStateEvent("Ping ok");
                    }
                    else
                    {
                        OnConnectStateEvent("Ping no");
                        WebSocket.Connect();
                    }
                }
                catch (Exception ex)
                {
                    OnConnectStateEvent($"Ping Error: {ex.Message}");
                }
            }
        }

        public void SocketOpen()
        {
            Disconnect();
            WebSocket = new WebSocket($"{Resources.SOCKET}{pair.ToLower()}@kline_{interval}");
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnClose += WebSocket_OnClose;
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.Connect();
        }

        public string GetHistory()
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
            OnConnectStateEvent($"Kline Connect");
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            OnConnectStateEvent($"Kline Disconnect");
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            OnConnectStateEvent($"Kline Error: {e.Message}");
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

        protected virtual void OnMessageEvent(KlineEventArgs e)
        {
            MessageEvent(this, e);
        }
        protected virtual void OnConnectStateEvent(string e)
        {
            ConnectStateEvent(this, e);
        }
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
