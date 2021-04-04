using StockExchenge.RestApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using WebSocketSharp;

namespace StockExchenge.Charts
{
    public class Kline
    {
        public WebSocket WebSocket { get; private set; }
        private readonly PublicRequester publicRequester;

        public event EventHandler<KlineEventArgs> MessageEvent;
        public event EventHandler<string> ConnectStateEvent;
        public event EventHandler ConnectEvent;

        readonly string pair;
        readonly string interval;
        readonly Timer timer;

        public Kline(string pair, string interval)
        {
            this.pair = pair;
            this.interval = interval;
            MessageEvent = delegate { };
            ConnectStateEvent = delegate { };
            ConnectEvent = delegate { };
            publicRequester = new PublicRequester();
            timer = new Timer(30000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (WebSocket != null)
            {
                try
                {
                    if (WebSocket.Ping())
                    {
                        //OnConnectStateEvent("Kline: Ping ok");
                    }
                    else
                    {
                        OnConnectStateEvent("Kline: Ping no");
                        SocketOpen();
                    }
                }
                catch (Exception ex)
                {
                    OnConnectStateEvent($"Kline: Ping Error: {ex.Message}");
                    SocketOpen();
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
            try
            {
                WebSocket.Connect();
            }
            catch (Exception ex)
            {

            }
        }

        public string GetHistory()
        {
            try
            {
                return publicRequester.RequestPublicApi($"{Resources.DOMAIN_V1}klines?symbol={pair.ToUpper()}&interval={interval}&limit=70");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public void Disconnect()
        {
            timer.Stop();
            if (WebSocket != null)
            {
                WebSocket.OnMessage -= WebSocket_OnMessage;
                WebSocket.OnError -= WebSocket_OnError;
                WebSocket.OnClose -= WebSocket_OnClose;
                WebSocket.OnOpen -= WebSocket_OnOpen;
                WebSocket.Close();
                WebSocket = null;
            }
            timer.Start();
        }

        #region// test
        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            OnConnectStateEvent($"Kline Connect");
            OnConnectEvent();
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            OnConnectStateEvent($"Kline Disconnect");
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            OnConnectStateEvent($"Kline Error: {e.Message}");
            SocketOpen();
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
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
        protected virtual void OnConnectEvent()
        {
            ConnectEvent(this, new EventArgs());
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
