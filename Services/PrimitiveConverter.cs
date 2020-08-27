using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Services
{
    public class PrimitiveConverter
    {
        public static double ToDouble(string value)
        {
            double result = 0;
            try
            {
                double.TryParse(value, NumberStyles.Number, new CultureInfo("en-US"), out result);
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
