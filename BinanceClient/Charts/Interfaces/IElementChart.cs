﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Charts.Interfaces
{
    /// <summary>
    /// Элемент основного графика: линии, точки, свечи и т.д.
    /// </summary>
    public interface IElementChart
    {
        /// <summary>
        /// Время открытия
        /// </summary>
        public long TimeOpen { get; }
        /// <summary>
        /// Максимум значения: цены и т.д.
        /// </summary>
        public double High { get; }
        /// <summary>
        /// Минимум значения: цены и т.д.
        /// </summary>
        public double Low { get; }
    }
}
