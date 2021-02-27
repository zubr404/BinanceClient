using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.Models
{
    public class ParametrBuySellView : PropertyChangedBase
    {
        public ParametrBuySellView()
        {
            Coins = new List<string>();
        }

        public List<string> Coins { get; set; }

        private string mainCoin = "BTC";
        public string MainCoin
        {
            get { return mainCoin; }
            set
            {
                mainCoin = value.ToUpper();
                base.NotifyPropertyChanged();
                SetIsEnabled();
            }
        }
        private string altCoin = "USDT";
        public string AltCoin
        {
            get { return altCoin; }
            set
            {
                altCoin = value.ToUpper();
                base.NotifyPropertyChanged();
                SetIsEnabled();
            }
        }

        private double price = 15000;
        public double Price
        {
            get { return price; }
            set
            {
                if(value > 0)
                {
                    price = value;
                    base.NotifyPropertyChanged();
                    SetIsEnabled();
                }
            }
        }

        private double amount = 1;
        public double Amount
        {
            get { return amount; }
            set
            {
                if(value > 0)
                {
                    amount = value;
                    base.NotifyPropertyChanged();
                    SetIsEnabled();
                }
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                base.NotifyPropertyChanged();
            }
        }

        public string GetPair()
        {
            return $"{MainCoin}{AltCoin}";
        }


        private void SetIsEnabled()
        {
            if(!string.IsNullOrWhiteSpace(MainCoin)
                && !string.IsNullOrWhiteSpace(AltCoin)
                && Price > 0
                && Amount > 0)
            {
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
            }
        }
    }
}
