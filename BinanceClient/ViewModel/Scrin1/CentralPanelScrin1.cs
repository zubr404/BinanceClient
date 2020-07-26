using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BinanceClient.Models;

namespace BinanceClient.ViewModel.Scrin1
{
    public class CentralPanelScrin1
    {
        public TradeConfigurationView TradeConfiguration { get; set; }
        readonly TradeConfigRepository configRepository;

        public CentralPanelScrin1(TradeConfigRepository configRepository)
        {
            TradeConfiguration = new TradeConfigurationView();
            this.configRepository = configRepository;
            // получаем сохраненные конфиги для правых кнопок
        }

        private RelayCommand applyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                return applyCommand ?? new RelayCommand((object o) =>
                {
                    var x = TradeConfiguration;
                    // сохраняем конфиг
                    // или обновляем для правых кнопок
                });
            }
        }
    }
}
