using DataBaseWork.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        readonly ConnectedPairRepository connectedPairRepository;
        public WebSocket WebSocket { get; private set; }

        public event EventHandler<CurrentTradeEventArgs> MessageEvent;
        public event EventHandler<string> ConnectStateEvent;
        public event EventHandler ConnectEvent;
        public event EventHandler<LastPriceEventArgs> LastPriceEvent;

        private List<string> pairs;
        private Timer timer;
        private Dictionary<string, double> previosPrices;

        public CurrentTrades(ConnectedPairRepository connectedPairRepository)
        {
            this.connectedPairRepository = connectedPairRepository;
            GetPairs();
            previosPrices = new Dictionary<string, double>();
            MessageEvent = delegate { };
            ConnectStateEvent = delegate { };
            ConnectEvent = delegate { };
            LastPriceEvent = delegate { };
            timer = new Timer(30000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void GetPairs()
        {
            pairs = new List<string>();
            foreach (var pair in connectedPairRepository.GetActive())
            {
                pairs.Add($"{pair.MainCoin}{pair.AltCoin}");
            }
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
            GetLastPrice(e.Data);
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

        private void GetLastPrice(string data)
        {
            var aggTrade = JConverter.JsonConver<AggTrade>(data);
            var lastPrice = PrimitiveConverter.ToDouble(aggTrade.p);

            if (!previosPrices.ContainsKey(aggTrade.s))
            {
                previosPrices.Add(aggTrade.s, lastPrice);
                OnLastPriceEvent(aggTrade.s, lastPrice);
            }
            else
            {
                var previosPrice = previosPrices[aggTrade.s];
                if(lastPrice != previosPrice)
                {
                    previosPrices[aggTrade.s] = lastPrice;
                    OnLastPriceEvent(aggTrade.s, lastPrice);
                }
            }
        }

        protected virtual void OnLastPriceEvent(string pair, double price)
        {
            LastPriceEvent(this, new LastPriceEventArgs(pair, price));
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

    public class LastPriceEventArgs : EventArgs
    {
        public string Pair { get; private set; }
        public double LastPrice { get; private set; }
        public LastPriceEventArgs(string pair, double price)
        {
            Pair = pair;
            LastPrice = price;
        }
    }
}
