using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms
{
    public class ExchangeSettingsPair
    {
        public int BasePrecision { get; set; }
        public int QuotePrecision { get; set; }
        public LotSizeFilter LotSizeFilter { get; set; }

        public ExchangeSettingsPair()
        {
            LotSizeFilter = new LotSizeFilter();
        }
    }

    public class LotSizeFilter
    {
        public decimal StepSize { get; set; }
    }
}
