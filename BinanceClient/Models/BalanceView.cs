using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.Models
{
    public class BalanceView : PropertyChangedBase
    {
        private string asset;
        public string Asset 
        {
            get { return asset; }
            set
            {
                asset = value;
                base.NotifyPropertyChanged();
            }
        }

        private double free;
        public double Free
        {
            get { return free; }
            set
            {
                free = value;
                base.NotifyPropertyChanged();
            }
        }

        private double locked;
        public double Locked
        {
            get { return locked; }
            set
            {
                locked = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
