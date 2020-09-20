using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BinanceClient.ViewModel.Scrin1
{
    /// <summary>
    /// Левая панель с кнопками
    /// </summary>
    public class LeftPanelScrin1 : IManagingButtonBackground
    {
        public IColorButton MainButton { get; set; }
        public IColorButton CurrentStatisticButton { get; set; }
        public IColorButton CalculatorButton { get; set; }
        public IColorButton GeneralStatisticsButton { get; set; }
        public IColorButton BackTestingButton { get; set; }

        private List<IColorButton> colorButtons;

        public LeftPanelScrin1()
        {
            colorButtons = new List<IColorButton>();
            MainButton = new BackgroundButton() { Name = ButtonName.MainButton};
            CurrentStatisticButton = new BackgroundButton() { Name = ButtonName.CurrentStatistic };
            CalculatorButton = new BackgroundButton() { Name = ButtonName.Calculator };
            GeneralStatisticsButton = new BackgroundButton() { Name = ButtonName.GeneralStatistics };
            BackTestingButton = new BackgroundButton() { Name = ButtonName.BackTesting };

            colorButtons.Add(MainButton);
            colorButtons.Add(CurrentStatisticButton);
            colorButtons.Add(CalculatorButton);
            colorButtons.Add(GeneralStatisticsButton);
            colorButtons.Add(BackTestingButton);
            InitializeColor();
        }

        public void ManagingBackground(ButtonName buttonName)
        {
            foreach (var item in colorButtons)
            {
                if(item.Name == buttonName)
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidYellow"];
                }
                else
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidGray"];
                }
            }
        }

        private void InitializeColor()
        {
            foreach (var item in colorButtons)
            {
                if(item.Name == ButtonName.MainButton)
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidYellow"];
                }
                else
                {
                    item.BrushBackground = (Brush)Application.Current.Resources["SolidGray"];
                }
            }
        }
    }
}
