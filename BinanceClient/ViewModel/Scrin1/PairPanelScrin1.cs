using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace BinanceClient.ViewModel.Scrin1
{
    public class PairPanelScrin1 : PropertyChangedBase
    {
        readonly ConnectedPairRepository connectedPairRepository;

        public ObservableCollection<ConnectedPairView> ConnectedPairs { get; set; }

        public PairPanelScrin1(ConnectedPairRepository connectedPairRepository)
        {
            this.connectedPairRepository = connectedPairRepository;
            ConnectedPairs = new ObservableCollection<ConnectedPairView>();
        }

        public void GetPairs()
        {
            ConnectedPairs.Clear();
            var pairs = connectedPairRepository.Get().OrderByDescending(x=>x.Active);
            foreach (var pair in pairs)
            {
                ConnectedPairs.Add(new ConnectedPairView()
                {
                    BaseAsset = pair.MainCoin,
                    QuoteAsset = pair.AltCoin,
                    IsActive = pair.Active
                });
            }
        }

        private RelayCommand applyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                return applyCommand ?? new RelayCommand((object o) =>
                {
                    var changedPairs = ConnectedPairs.Where(x => x.IsChangeActive);
                    foreach (var changedPair in changedPairs)
                    {
                        connectedPairRepository.Update(new ConnectedPair()
                        {
                            MainCoin = changedPair.BaseAsset,
                            AltCoin = changedPair.QuoteAsset,
                            Active = changedPair.IsActive
                        });
                    }
                    MessageBox.Show("Что бы изменения вступили в силу нужно перезапустить программу.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        }
    }
}
