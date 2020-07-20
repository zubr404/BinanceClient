using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace BinanceClient.ViewModel
{
    public class BackgroundButton : PropertyChangedBase, IColorButton
    {
        private Brush brushBackground;
        public Brush BrushBackground
        {
            get { return brushBackground; }
            set
            {
                brushBackground = value;
                base.NotifyPropertyChanged();
            }
        }

        public ButtonName Name { get; set; }
    }
}
