using System;
using System.Collections.Generic;
using System.Text;

namespace z_ChartAppTest.Interfaces
{
    /// <summary>
    /// График, например, японская свеча или бар
    /// </summary>
    public interface IChart
    {
        /// <summary>
        /// Создать график
        /// </summary>
        /// <param name="maxAllChart">Максимум по всем значениям графика</param>
        /// <param name="scaleIntervalPrice">цена деления шкалы Price</param>
        public void Create(double maxAllChart, double scaleIntervalPrice);
    }
}
