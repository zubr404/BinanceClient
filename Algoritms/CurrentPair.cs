using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms
{
    public class CurrentPair
    {
        public string BaseAsset { get; private set; }
        public string QuoteAsset { get; private set; }
        public string Pair { get; private set; }

        public CurrentPair(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            SetCurrentPair();
        }

        private void SetCurrentPair()
        {
            Pair = $"{BaseAsset}{QuoteAsset}".ToUpper();
        }
    }
}
