using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class TimeConverter
    {
        public static string ToStringTime(DateTime time)
        {
            return $"{time.Year}-{time.Month:00}-{time.Day:00} {time.Hour:00}:{time.Minute:00}:{time.Second:00}";
        }
    }
}
