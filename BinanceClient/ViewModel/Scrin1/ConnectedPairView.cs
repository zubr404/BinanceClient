using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceClient.ViewModel.Scrin1
{
    public class ConnectedPairView : PropertyChangedBase
    {
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
            get
            {
                return quoteAsset;
            }
            set
            {
                quoteAsset = value;
                base.NotifyPropertyChanged();
            }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                SetCountChangeActive();
                base.NotifyPropertyChanged();
            }
        }

        public bool IsChangeActive { get; private set; }

        private int countChangeActive;
        private void SetCountChangeActive()
        {
            if(countChangeActive > 0)
            {
                IsChangeActive = true;
            }
            countChangeActive++;
        }
    }
}
