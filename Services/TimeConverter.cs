using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class TimeConverter
    {
        public static string ToStringTime(DateTime time)
        {
            return $"{time.Year}-{time.Month.ToString("00")}-{time.Day.ToString("00")} {time.Hour.ToString("00")}:{time.Minute.ToString("00")}:{time.Second.ToString("00")}";
        }
    }
}
