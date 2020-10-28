using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;

namespace BinanceClient.Models
{
    public class TradeConfigurationView : PropertyChangedBase
    {
        public TradeConfigurationView()
        {
            Coins = new List<string>();
        }

        public List<string> Coins { get; set; }

        private string pair;
        public string Pair
        {
            get { return pair; }
            set 
            { 
                pair = value;
                base.NotifyPropertyChanged();
            }
        }



        private string mainCoin;
        public string MainCoin 
        {
            get { return mainCoin; }
            set
            {
                mainCoin = value.ToUpper();
                base.NotifyPropertyChanged();
            }
        }
        private string altCoin;
        public string AltCoin 
        {
            get { return altCoin; }
            set
            {
                altCoin = value.ToUpper();
                base.NotifyPropertyChanged();
            }
        }
        private string strategy = "Long";
        public string Strategy 
        {
            get { return strategy; }
            set
            {
                strategy = value;
                base.NotifyPropertyChanged();
            }
        }
        private int intervalHttp;
        public int IntervalHttp 
        {
            get { return intervalHttp; }
            set
            {
                if(value >= 0)
                {
                    intervalHttp = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private bool margin;
        public bool Margin 
        {
            get { return margin; }
            set
            {
                margin = value;
                base.NotifyPropertyChanged();
            }
        }
        private int openOrders;
        public int OpenOrders 
        {
            get { return openOrders; }
            set
            {
                if(value > 0)
                {
                    openOrders = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double orderIndent;
        public double OrderIndent
        {
            get { return orderIndent; }
            set
            {
                if (value > 0)
                {
                    orderIndent = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double orderDeposit;
        public double OrderDeposit
        {
            get { return orderDeposit; }
            set
            {
                if (value > 0)
                {
                    orderDeposit = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double firstStep;
        public double FirstStep
        {
            get { return firstStep; }
            set
            {
                if (value > 0)
                {
                    firstStep = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double orderStepPlus;
        public double OrderStepPlus
        {
            get { return orderStepPlus; }
            set
            {
                if (value > 0)
                {
                    orderStepPlus = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double martingale;
        public double Martingale
        {
            get { return martingale; }
            set
            {
                if (value > 0)
                {
                    martingale = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double depositLimit;
        public double DepositLimit
        {
            get { return depositLimit; }
            set
            {
                if (value > 0)
                {
                    depositLimit = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double orderReload;
        public double OrderReload
        {
            get { return orderReload; }
            set
            {
                if (value > 0)
                {
                    orderReload = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double loss;
        public double Loss
        {
            get { return loss; }
            set
            {
                if (value >= 0)
                {
                    loss = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double profit;
        public double Profit
        {
            get { return profit; }
            set
            {
                if (value > 0)
                {
                    profit = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double indentExtremum;
        public double IndentExtremum
        {
            get { return indentExtremum; }
            set
            {
                if (value >= 0)
                {
                    indentExtremum = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
        private double protectiveSpread;
        public double ProtectiveSpread
        {
            get { return protectiveSpread; }
            set
            {
                if (value > 0)
                {
                    protectiveSpread = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
    }
}
