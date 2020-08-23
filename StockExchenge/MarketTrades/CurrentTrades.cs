using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using WebSocketSharp;

namespace StockExchenge.MarketTrades
{
    /// <summary>
    /// Информация в реальном времени о текущих сделках на бирже
    /// </summary>
    public class CurrentTrades
    {
        public WebSocket WebSocket { get; private set; }

        public event EventHandler<CurrentTradeEventArgs> MessageEvent;
        public event EventHandler<string> ConnectStateEvent;
        public event EventHandler ConnectEvent;

        readonly string[] pairs;
        private Timer timer;

        public CurrentTrades(string[] pairs)
        {
            this.pairs = pairs;
            MessageEvent = delegate { };
            ConnectStateEvent = delegate { };
            ConnectEvent = delegate { };
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
                        OnConnectStateEvent("CurrentTrades: Ping ok");
                    }
                    else
                    {
                        OnConnectStateEvent("CurrentTrades: Ping no");
                        WebSocket.Connect();
                    }
                }
                catch (Exception ex)
                {
                    OnConnectStateEvent($"CurrentTrades: Ping Error: {ex.Message}");
                }
            }
        }

        public void SocketOpen()
        {
            Disconnect();
            string pairParams = "";
            foreach (var pair in pairs)
            {
                pairParams += $"{pair.ToLower()}@aggTrade/";
            }
            pairParams = pairParams.Remove(pairParams.Length - 1);

            WebSocket = new WebSocket($"{Resources.SOCKET}{pairParams}");
            WebSocket.OnMessage += WebSocket_OnMessage;
            WebSocket.OnError += WebSocket_OnError;
            WebSocket.OnClose += WebSocket_OnClose;
            WebSocket.OnOpen += WebSocket_OnOpen;
            WebSocket.Connect();
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

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            OnConnectStateEvent($"CurrentTrades: Connect");
            OnConnectEvent();
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            OnConnectStateEvent($"CurrentTrades: Disconnect");
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            OnConnectStateEvent($"CurrentTrades Error: {e.Message}");
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            OnMessageEvent(new CurrentTradeEventArgs(e.Data));
        }

        protected virtual void OnMessageEvent(CurrentTradeEventArgs e)
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

    public class CurrentTradeEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public CurrentTradeEventArgs(string message)
        {
            Message = message;
        }
    }
}
