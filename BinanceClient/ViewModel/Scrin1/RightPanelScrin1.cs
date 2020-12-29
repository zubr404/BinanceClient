using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly TradeConfigRepository configRepository;

        public RightPanelScrin1()
        {
            configRepository = new TradeConfigRepository();
            BTCUSD_LONG = new BackgroundButton() { Name = ButtonName.BTCUSD_LONG };
            BTCUSD_SHORT = new BackgroundButton() { Name = ButtonName.BTCUSD_SHORT };
            ETHUSD_SHORT = new BackgroundButton() { Name = ButtonName.ETHUSD_SHORT };
            colorButtons = new List<IColorButton>();
            colorButtons.Add(BTCUSD_LONG);
            colorButtons.Add(BTCUSD_SHORT);
            colorButtons.Add(ETHUSD_SHORT);
            InicializeColor();
        }

        /* Прописано жестко для каждой кнопки
         * пока не понятно, как это будет выглядеть в будуюзем
         */

        public void ManagingBackground(ButtonName buttonName)
        {
            TradeConfiguration config;
            switch (buttonName)
            {
                case ButtonName.BTCUSD_LONG:
                    config = configRepository.Get("BTC", "USDT", "LONG");
                    if(config != null)
                    {
                        config.Active = !config.Active;
                        var configNew = configRepository.Update(config, Resources.SAVED_STRATEGIES);
                        UpdateBrushBackground(buttonName, configNew.Active);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    break;
                case ButtonName.BTCUSD_SHORT:
                    config = configRepository.Get("BTC", "USDT", "SHORT");
                    if (config != null)
                    {
                        config.Active = !config.Active;
                        var configNew = configRepository.Update(config, Resources.SAVED_STRATEGIES);
                        UpdateBrushBackground(buttonName, configNew.Active);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    break;
                case ButtonName.ETHUSD_SHORT:
                    config = configRepository.Get("ETH", "USDT", "SHORT");
                    if (config != null)
                    {
                        config.Active = !config.Active;
                        var configNew = configRepository.Update(config, Resources.SAVED_STRATEGIES);
                        UpdateBrushBackground(buttonName, configNew.Active);
                    }
                    else
                    {
                        MessageBox.Show("Для данной стратегии не найдена сохраненная конфигурация.", "Конфигурация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateBrushBackground(ButtonName buttonName, bool isActive)
        {
            var button = colorButtons.First(x => x.Name == buttonName);
            if (isActive)
            {
                button.BrushBackground = (Brush)Application.Current.Resources["SolidGreen"];
            }
            else
            {
                button.BrushBackground = (Brush)Application.Current.Resources["SolidWhite"];
            }
        }

        private void InicializeColor()
        {
            foreach (var item in colorButtons)
            {
                TradeConfiguration config;
                switch (item.Name)
                {
                    case ButtonName.BTCUSD_LONG:
                        config = configRepository.Get("BTC", "USDT", "LONG");
                        if (config != null)
                        {
                            UpdateBrushBackground(item.Name, config.Active);
                        }
                        else
                        {
                            UpdateBrushBackground(item.Name, false);
                        }
                        break;
                    case ButtonName.BTCUSD_SHORT:
                        config = configRepository.Get("BTC", "USDT", "SHORT");
                        if (config != null)
                        {
                            UpdateBrushBackground(item.Name, config.Active);
                        }
                        else
                        {
                            UpdateBrushBackground(item.Name, false);
                        }
                        break;
                    case ButtonName.ETHUSD_SHORT:
                        config = configRepository.Get("ETH", "USDT", "SHORT");
                        if (config != null)
                        {
                            UpdateBrushBackground(item.Name, config.Active);
                        }
                        else
                        {
                            UpdateBrushBackground(item.Name, false);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
