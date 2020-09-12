using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel.Scrin1
{
    /// <summary>
    /// Консоль сообщений главного экрана
    /// </summary>
    public class ConsoleScrin1 : PropertyChangedBase
    {
        private string message = "";
        public string Message
        {
            get { return message; }
            set
            {
                //message += $"{DateTime.Now} {value}\n";
                message = message.Insert(0, $"{DateTime.Now} {value}\n");
                if(message.Length > 20000)
                {
                    message = message.Remove(15000);
                }
                base.NotifyPropertyChanged();
            }
        }
    }
}
