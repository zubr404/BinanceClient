using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Charts.Interfaces
{
    /// <summary>
    /// График, например, японская свеча или бар
    /// </summary>
    public interface IChart
    {
        public ObservableCollection<IElementChartView> ElementChartViews { get; }
        /// <summary>
        /// Создать график
        /// </summary>
        /// <param name="maxAllChart">Максимум по всем значениям графика</param>
        /// <param name="scaleIntervalPrice">цена деления шкалы Price</param>
        public void Create(double maxAllChart, double scaleIntervalPrice);
    }
}
