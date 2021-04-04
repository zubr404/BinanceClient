using BinanceClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charts.Services
{
    public class TestMonitor : PropertyChangedBase
    {
        private string textMonitor;
        public string TextMonitor
        {
            get { return textMonitor; }
            set
            {
                textMonitor = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
