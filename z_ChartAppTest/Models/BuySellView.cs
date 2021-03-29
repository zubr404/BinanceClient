using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Models
{
    /// <summary>
    /// Для отображения заявок/сделок
    /// </summary>
    public class BuySellView
    {
        public double Price { get; set; }
        public bool IsBuy { get; set; }
    }
}
