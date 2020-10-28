using DataBaseWork.Models;
using DataBaseWork.Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using WebSocketSharp;

namespace StockExchenge.StreamWs
{
    /// <summary>
    /// Поток с изменениями
    /// </summary>
    public class UserStreamData
    {
        readonly RepositoriesModel repositories;
        readonly List<WebSocketUser> webSockets;
        private Timer timer;
        private IEnumerable<APIKey> keys;
        public UserStreamData(RepositoriesModel repositories)
        {
            this.repositories = repositories;
            webSockets = new List<WebSocketUser>();
            timer = new Timer(1800000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StreamKeppAlive();
        }

        public void StreamStart()
        {
            keys = GetApiKeys();
            foreach (var key in keys)
            {
                var listenKey = ListenKeyParse(GetListenKeyUserStream(key.PublicKey));
                if (!string.IsNullOrWhiteSpace(listenKey))
                {
                    webSockets.Add(new WebSocketUser(repositories, listenKey, key));
                }
            }
        }

        private void StreamKeppAlive()
        {
            if(keys != null)
            {
                foreach (var key in keys)
                {
                   GetListenKeyUserStream(key.PublicKey, "PUT");
                }
            }
        }

        private IEnumerable<APIKey> GetApiKeys()
        {
            return repositories.APIKeyRepository.Get();
        }

        // POST /api/v3/userDataStream - Запустить новый поток пользовательских данных.
        // PUT /api/v3/userDataStream - Потоки пользовательских данных закроются через 60 минут. Рекомендуется отправлять пинг каждые 30 минут.
        private string GetListenKeyUserStream(string key, string method = "POST")
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"{Resources.DOMAIN_V3}userDataStream");
                req.Method = method;
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                req.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Headers.Add("X-MBX-APIKEY", key);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                return "";
                //throw ex;
            }
        }

        private string ListenKeyParse(string data)
        {
            return data.Replace("listenKey", "").Replace("{", "").Replace("}", "").Replace("\"", "").Replace(":", "").Replace(" ", "");
        }
    }

    public class WebSocketUser
    {
        readonly WebSocket webSocket;
        readonly RepositoriesModel repositories;
        readonly APIKey key;

        public WebSocketUser(RepositoriesModel repositories, string listenKey, APIKey key)
        {
            this.repositories = repositories;
            this.key = key;
            webSocket = new WebSocket($"{Resources.SOCKET}{listenKey}");
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.Connect();
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Data.Contains("outboundAccountPosition")) // Дополнительное событие outboundAccountPosition отправляется каждый раз при изменении баланса счета и содержит активы, которые, возможно, были изменены событием, вызвавшим изменение баланса.
            {
                UpdateBalance(e.Data);
            }
        }

        private void UpdateBalance(string data)
        {
            var balance = JConverter.JsonConver<BalanceInfo>(data);
            
            if (balance != null)
            {
                if (balance.B != null)
                {
                    foreach (var b in balance.B)
                    {
                        if (b.f != 0 || b.l != 0)
                        {
                            repositories.BalanceRepository.Update(new DataBaseWork.Models.Balance()
                            {
                                FK_PublicKey = key.PublicKey,
                                Asset = b.a,
                                Free = b.f,
                                Locked = b.l
                            });
                        }
                    }
                }
            }
        }
    }
}
