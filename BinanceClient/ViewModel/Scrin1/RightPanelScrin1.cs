using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BinanceClient.ViewModel.Scrin1
{
    public class RightPanelScrin1 : IManagingButtonBackground
    {
        public IColorButton BTCUSD_LONG { get; set; }
        public IColorButton BTCUSD_SHORT { get; set; }
        public IColorButton ETHUSD_SHORT { get; set; }
        private List<IColorButton> colorButtons;

        public RightPanelScrin1()
        {
            BTCUSD_LONG = new BackgroundButton() { Name = ButtonName.BTCUSD_LONG };
            BTCUSD_SHORT = new BackgroundButton() { Name = ButtonName.BTCUSD_SHORT };
            ETHUSD_SHORT = new BackgroundButton() { Name = ButtonName.ETHUSD_SHORT };
            colorButtons = new List<IColorButton>();
            colorButtons.Add(BTCUSD_LONG);
            colorButtons.Add(BTCUSD_SHORT);
            colorButtons.Add(ETHUSD_SHORT);
            InicializeColor();
        }

        public void ManagingBackground(ButtonName buttonName)
        {
            foreach (var item in colorButtons)
            {
                if (item.Name == buttonName)
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidGreen"];
                }
                else
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidWhite"];
                }
            }
        }

        private void InicializeColor()
        {
            foreach (var item in colorButtons)
            {
                item.BrushBackground = (Brush)Application.Current.Resources["SolidWhite"];
            }
        }
    }
}
