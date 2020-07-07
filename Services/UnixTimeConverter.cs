using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public static class UnixTimeConverter
    {
        public static DateTime ConvertUnixTime(this long timestamp)
        {

            return (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddMilliseconds(Math.Abs(timestamp));
        }
    }
}
