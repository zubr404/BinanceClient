using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BinanceClient.ViewModel
{
    public class Scrin : PropertyChangedBase
    {
        public ScrinName Name { get; set; }

        private Visibility visibility;
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
