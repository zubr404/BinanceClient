using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel
{
    public class ConsoleScrin1 : PropertyChangedBase
    {
        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message += $"{DateTime.Now} {value}\n";
                base.NotifyPropertyChanged();
            }
        }
    }
}
