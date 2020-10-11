using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel.ScrinCalculator
{
    public class GeneralSettingsView : PropertyChangedBase
    {
        private DateTime startTime = new DateTime(2017,1,6);
        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                base.NotifyPropertyChanged();
            }
        }

        private DateTime stopTime = new DateTime(2017, 7, 19);
        public DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                stopTime = value;
                base.NotifyPropertyChanged();
            }
        }

        private double depositAsset = 100;
        public double DepositAsset
        {
            get { return depositAsset; }
            set
            {
                if (value > 0)
                {
                    depositAsset = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private double depositQuote = 100;
        public double DepositQuote
        {
            get { return depositQuote; }
            set
            {
                if (value > 0)
                {
                    depositQuote = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private double depositAssetForCurrentGrid = 75;
        public double DepositAssetForCurrentGrid
        {
            get { return depositAssetForCurrentGrid; }
            set
            {
                if (value > 0)
                {
                    depositAssetForCurrentGrid = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private double depositQuoteForCurrentGrid = 75;
        public double DepositQuoteForCurrentGrid
        {
            get { return depositQuoteForCurrentGrid; }
            set
            {
                if (value > 0)
                {
                    depositQuoteForCurrentGrid = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private string baseAsset;
        public string BaseAsset
        {
            get { return baseAsset; }
            set
            {
                baseAsset = value;
                base.NotifyPropertyChanged();
            }
        }

        private string quoteAsset;
        public string QuoteAsset
        {
            get { return quoteAsset; }
            set
            {
                quoteAsset = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
