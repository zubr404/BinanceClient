using BinanceClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BinanceClient
{
    public class ScrinManager : PropertyChangedBase
    {
        public Scrin Scrin1 { get; set; }
        public Scrin ScrinCurrentStatistic { get; set; }
        public Scrin ScrinCalculator { get; set; }
        public Scrin ScrinGeneralStatistic { get; set; }
        public Scrin ScrinBacktesting { get; set; }
        public Scrin ScrinPairConnected { get; set; }

        private List<Scrin> scrins;

        public ScrinManager()
        {
            scrins = new List<Scrin>();
            Scrin1 = new Scrin() { Name = ScrinName.Scrin1 };
            ScrinCurrentStatistic = new Scrin() { Name = ScrinName.ScrinCurrentStatistic };
            ScrinCalculator = new Scrin() { Name = ScrinName.ScrinCalculator };
            ScrinGeneralStatistic = new Scrin() { Name = ScrinName.ScrinGeneralStatistic };
            ScrinBacktesting = new Scrin() { Name = ScrinName.ScrinBacktesting };
            ScrinPairConnected = new Scrin() { Name = ScrinName.ScrinPairConnected };
            scrins.Add(Scrin1);
            scrins.Add(ScrinCurrentStatistic);
            scrins.Add(ScrinCalculator);
            scrins.Add(ScrinGeneralStatistic);
            scrins.Add(ScrinBacktesting);
            scrins.Add(ScrinPairConnected);
            InitializeScrin();
        }

        public void ManagingScrin(ScrinName scrinName)
        {
            foreach (var scrin in scrins)
            {
                if (scrin.Name == scrinName)
                {
                    scrin.Visibility = Visibility.Visible;
                }
                else
                {
                    scrin.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void InitializeScrin()
        {
            foreach (var scrin in scrins)
            {
                if(scrin.Name == ScrinName.Scrin1)
                {
                    scrin.Visibility = Visibility.Visible;
                }
                else
                {
                    scrin.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
