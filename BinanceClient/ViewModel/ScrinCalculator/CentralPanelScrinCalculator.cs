using Algoritms.BackTest;
using BinanceClient.ViewModel.Scrin1;
using DataBaseWork.Repositories;
using StockExchenge.MarketTradesHistory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BinanceClient.ViewModel.ScrinCalculator
{
    public class CentralPanelScrinCalculator : PropertyChangedBase
    {
        readonly Martingale martingale;
        readonly CurrentGridStatistics currentGridStatistics;
        public GeneralSettingsView GeneralSettingsView { get; set; }
        public BackTestResultView BackTestResultView { get; set; }

        private TradesHistory tradesHistory;
        readonly TradeHistoryRepository tradeHistoryRepository;

        public CentralPanelScrinCalculator(Martingale martingale, CurrentGridStatistics currentGridStatistics, TradeHistoryRepository tradeHistoryRepository)
        {
            this.martingale = martingale;
            this.currentGridStatistics = currentGridStatistics;
            this.tradeHistoryRepository = tradeHistoryRepository;
            GeneralSettingsView = new GeneralSettingsView();
            BackTestResultView = new BackTestResultView();
        }

        private bool isCheckedBackTest;
        public bool IsCheckedBackTest
        {
            get { return isCheckedBackTest; }
            set
            {
                isCheckedBackTest = value;
                base.NotifyPropertyChanged();
            }
        }

        Task currentGridCaclTask;
        private void StartBackTest()
        {
            if(currentGridCaclTask != null)
            {
                if (!currentGridCaclTask.IsCompleted)
                {
                    return;
                }
            }
            currentGridCaclTask = Task.Run(() =>
            {
                var statistcs = martingale.StartBackTest(GeneralSettingsView.StartTime, GeneralSettingsView.StopTime, GeneralSettingsView.DepositAsset, GeneralSettingsView.DepositQuote);
                BackTestResultView.CountProfit = statistcs.CountProfit;
                BackTestResultView.CountProfit = statistcs.CountLoss;
                BackTestResultView.MaxProfit = statistcs.MaxProfit;
                BackTestResultView.MaxLoss = statistcs.MaxLoss;
                BackTestResultView.TotalGross = statistcs.TotalGross;
                BackTestResultView.AvgProfitTrade = statistcs.AvgProfitTrade;
                BackTestResultView.AvgLossTrade = statistcs.AvgLossTrade;
                BackTestResultView.StdDevProfit = statistcs.StdDevProfit;
                IsCheckedBackTest = false;
            });
        }
        private void StopBackTest()
        {
            martingale.IsActiveCalculating = false;
        }

        private RelayCommand testStartCommand;
        public RelayCommand TestStartCommand
        {
            get
            {
                return testStartCommand ?? new RelayCommand((object o) =>
                {
                    if (isCheckedBackTest)
                    {
                        StartBackTest();
                    }
                    else
                    {
                        StopBackTest();
                    }
                });
            }
        }

        #region DownloadHistory
        private bool isCheckedDownloadHistory;
        public bool IsCheckedDownloadHistory
        {
            get { return isCheckedDownloadHistory; }
            set
            {
                isCheckedDownloadHistory = value;
                base.NotifyPropertyChanged();
            }
        }

        private bool isLoadAll;
        public bool IsLoadAll
        {
            get { return isLoadAll; }
            set
            {
                isLoadAll = value;
                base.NotifyPropertyChanged();
            }
        }

        // TODO: пара с морды
        private RelayCommand tradeHistoryLoad;
        public RelayCommand TradeHistoryLoad
        {
            get
            {
                return tradeHistoryLoad ?? new RelayCommand((object o) =>
                {
                    if (isCheckedDownloadHistory)
                    {
                        StartLoadHistory();
                    }
                    else
                    {
                        StopLoadHistory();
                    }
                });
            }
        }

        private DateTime? DateConvert(string donloadHistoryDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(donloadHistoryDate))
                {
                    return null;
                }
                return Convert.ToDateTime(donloadHistoryDate);
            }
            catch
            {
                return null;
            }
        }

        private async Task<int> StartLoadHistory()
        {
            int result;
            var dateStart = DateConvert(GeneralSettingsView.DateStart);
            var dateEnd = DateConvert(GeneralSettingsView.DateEnd);

            if (!isLoadAll)
            {
                if (!dateStart.HasValue)
                {
                    MessageBox.Show("Введена неверная стартовая дата.");
                    IsCheckedDownloadHistory = false;
                    return -1;
                }
                if (!dateEnd.HasValue)
                {
                    MessageBox.Show("Введена неверная конечная дата.");
                    IsCheckedDownloadHistory = false;
                    return -1;
                }
                if (dateStart.Value > dateEnd.Value)
                {
                    MessageBox.Show("Начальная дата больше конечной.");
                    IsCheckedDownloadHistory = false;
                    return -1;
                }
                if (dateStart.Value > DateTime.Now)
                {
                    MessageBox.Show("Начальная дата: введена дата из будущего.");
                    IsCheckedDownloadHistory = false;
                    return -1;
                }
                tradesHistory = new TradesHistory($"{GeneralSettingsView.BaseAsset}{GeneralSettingsView.QuoteAsset}", dateStart, dateEnd, tradeHistoryRepository);
            }
            else
            {
                tradesHistory = new TradesHistory($"{GeneralSettingsView.BaseAsset}{GeneralSettingsView.QuoteAsset}", null, null, tradeHistoryRepository);
            }
            
            tradesHistory.LoadStateEvent += TradesHistory_LoadStateEvent;
            result = await tradesHistory.Load();
            IsCheckedDownloadHistory = false;
            return result;
        }

        private void TradesHistory_LoadStateEvent(object sender, string e)
        {
            ModelView.ConsoleScrin1.Message = e;
        }

        private void StopLoadHistory()
        {
            if (tradesHistory != null)
            {
                tradesHistory.IsActiveLoad = false;
            }
        }
        #endregion

        #region CalcCurrentGrid
        private RelayCommand calcGridStart;
        public RelayCommand CalcGridStart
        {
            get
            {
                return calcGridStart ?? new RelayCommand((object o) =>
                {
                    CalcCurrentGrid(GeneralSettingsView.DepositAssetForCurrentGrid, GeneralSettingsView.DepositQuoteForCurrentGrid);
                });
            }
        }
        private void CalcCurrentGrid(double depositAsset, double depositQuote)
        {
            var orders = martingale.CalcCurrentGrid(depositAsset, depositQuote);
            List<OrderDataView> orderDataViews = new List<OrderDataView>();
            foreach (var orderData in currentGridStatistics.CalcGrid(orders))
            {
                orderDataViews.Add(new OrderDataView()
                {
                    Amount = orderData.Amount,
                    Equivalent = orderData.Equivalent,
                    PriceInGrid = orderData.PriceInGrid,
                    ProfitPrice = orderData.ProfitPrice,
                    Rebount = orderData.Rebount
                });
            }
            BackTestResultView.CurrentGridDatas = orderDataViews;
            BackTestResultView.DepositUsed = currentGridStatistics.DepositUsed;
            BackTestResultView.NumberOfOrders = currentGridStatistics.NumberOfOrders;
            BackTestResultView.CoverageOfPriceReduction = currentGridStatistics.CoverageOfPriceReduction;
        }
        #endregion
    }
}
