using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.REST
{
    class UserStreamData
    {
        public string GetListenKeyUserStream()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"https://api.binance.com/api/v3/userDataStream");
                req.Method = "POST";
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                req.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Headers.Add("X-MBX-APIKEY", "ztAZCxiv8UEJYU146zdTokXvqB3ygUHFAKbZxBxadZpqw7EZS8pUG9Yos0BezsNO");

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        WebSocketSharp.WebSocket webSocket;
        public void UserStreamWebSocket(string listenKey)
        {
            webSocket = new WebSocketSharp.WebSocket($"wss://stream.binance.com:9443/ws/{listenKey}");
            webSocket.OnMessage += (sender, e) =>
            {
                string jsonLine = e.Data.Replace(",[]", "");
                Console.WriteLine(jsonLine);

            };
            webSocket.OnError += (sender, e) => Console.WriteLine(e.Message);
            webSocket.OnOpen += (sender, e) => Console.WriteLine("Open");
            webSocket.Connect();
        }

        private string DigitalSignature(string api_parameters, string secret)
        {
            var keyByte = Encoding.UTF8.GetBytes(secret);

            string sign1 = string.Empty;
            byte[] inputBytes = Encoding.UTF8.GetBytes(api_parameters);
            using (var hmac = new HMACSHA256(keyByte))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);

                StringBuilder hex1 = new StringBuilder(hashValue.Length * 2);
                foreach (byte b in hashValue)
                {
                    hex1.AppendFormat("{0:x2}", b);
                }
                return sign1 = hex1.ToString();
            }
        }
    }
}
