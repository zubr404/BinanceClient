using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace BinanceClient
{
    public interface IColorButton
    {
        ButtonName Name { get; set; }
        Brush BrushBackground { get; set; }
    }
}
