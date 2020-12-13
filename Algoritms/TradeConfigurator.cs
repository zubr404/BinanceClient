using Algoritms.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms
{
    class TradeConfigurator
    {
        public Dictionary<string, TradeConfigurationAlgo> TradeConfigurations { get; private set; }

        public TradeConfigurator()
        {
            TradeConfigurations = new Dictionary<string, TradeConfigurationAlgo>();
        }

        public void SetConfigurations(IEnumerable<TradeConfigurationAlgo> configs)
        {
            TradeConfigurations = new Dictionary<string, TradeConfigurationAlgo>();
            foreach (var config in configs)
            {
                TradeConfigurations.Add(config.GetPair(), config);
            }
        }
    }
}
