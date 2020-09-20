using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DataBaseWork.Models
{
    public class ConnectedPairComparer : IEqualityComparer<ConnectedPair>

    {
        public bool Equals([AllowNull] ConnectedPair x, [AllowNull] ConnectedPair y)
        {
            if (x.MainCoin == y.MainCoin && x.AltCoin == y.AltCoin)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode([DisallowNull] ConnectedPair obj)
        {
            return $"{obj.MainCoin}{obj.AltCoin}".GetHashCode();
        }
    }
}
