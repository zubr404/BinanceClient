using BinanceClient.Models;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BinanceClient.ViewModel.Scrin1
{
    public class KeyPanelScrin1 : PropertyChangedBase
    {
        readonly APIKeyRepository apiKeyRepository;
        readonly BalanceRepository balanceRepository;
        public KeyPanelScrin1(APIKeyRepository apiKeyRepository, BalanceRepository balanceRepository)
        {
            this.apiKeyRepository = apiKeyRepository;
            this.balanceRepository = balanceRepository;
        }

        private List<APIKeyView> aPIKeyViews;
        public List<APIKeyView> APIKeyViews 
        {
            get { return aPIKeyViews; }
            set
            {
                if (value != null)
                {
                    aPIKeyViews = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private APIKeyView selectedKey;
        public APIKeyView SelectedKey
        {
            get { return selectedKey; }
            set
            {
                selectedKey = value;
                base.NotifyPropertyChanged();
            }
        }

        public void SetKeys()
        {
            var keys = apiKeyRepository.Get();
            var keysView = new List<APIKeyView>();
            foreach (var item in keys)
            {
                var key = new APIKeyView()
                {
                    ID = item.ID,
                    Name = item.Name,
                    PublicKey = item.PublicKey,
                    SecretKey = item.SecretKey,
                    IsActive = item.IsActive
                };
                key.SetStatus(item.Status);
                keysView.Add(key);
            }
            APIKeyViews = keysView;
        }

        #region Command
        private RelayCommand applyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                return applyCommand ?? new RelayCommand((object o) =>
                {
                    if(APIKeyViews != null)
                    {
                        foreach (var keyView in APIKeyViews)
                        {
                            apiKeyRepository.Update(new APIKey()
                            {
                                Name = keyView.Name,
                                PublicKey = keyView.PublicKey,
                                SecretKey = keyView.SecretKey,
                                IsActive = keyView.IsActive,
                                //Status = keyView.Status == StatusKey.OK.ToString()
                                Status = keyView.Status
                            });
                        }
                    }
                });
            }
        }

        private RelayCommand addKeyCommand;
        public RelayCommand AddKeyCommand
        {
            get
            {
                return addKeyCommand ?? new RelayCommand((object o) =>
                {
                    var key = new APIKey()
                    {
                        Name = UserName,
                        PublicKey = PublicKey,
                        SecretKey = SecretKey,
                        IsActive = true,
                        Status = true
                    };
                    try
                    {
                        apiKeyRepository.Create(key);
                        SetKeys();
                        UserName = "";
                        PublicKey = "";
                        SecretKey = "";
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
        }
        #endregion

        #region Панель отображения ключей
        private Visibility visibilityShowPanelKey = Visibility.Hidden;
        public Visibility VisibilityShowPanelKey
        {
            get { return visibilityShowPanelKey; }
            set
            {
                visibilityShowPanelKey = value;
                base.NotifyPropertyChanged();
            }
        }

        private RelayCommand showPanelKeyCommand;
        public RelayCommand ShowPanelKeyCommand
        {
            get
            {
                return showPanelKeyCommand ?? new RelayCommand((object o) =>
                {
                    VisibilityShowPanelKey = Visibility.Visible;
                    PublicKeyShow = SelectedKey != null ? SelectedKey.PublicKey : "---------------NaN---------------";
                    SecretKeyShow = SelectedKey != null ? SelectedKey.SecretKey : "---------------NaN---------------";
                });
            }
        }

        private RelayCommand hidePanelKeyCommand;
        public RelayCommand HidePanelKeyCommand
        {
            get
            {
                return hidePanelKeyCommand ?? new RelayCommand((object o) =>
                {
                    VisibilityShowPanelKey = Visibility.Hidden;
                    PublicKeyShow = "---------------NaN---------------";
                    SecretKeyShow = "---------------NaN---------------";
                });
            }
        }

        private string publicKeyShow = "---------------NaN---------------";
        public string PublicKeyShow
        {
            get { return publicKeyShow; }
            set
            {
                publicKeyShow = value;
                base.NotifyPropertyChanged();
            }
        }

        private string secretKeyShow = "---------------NaN---------------";
        public string SecretKeyShow
        {
            get { return secretKeyShow; }
            set
            {
                secretKeyShow = value;
                base.NotifyPropertyChanged();
            }
        }
        #endregion

        #region Панель отображения балансов
        private List<BalanceView> balanceViews;
        public List<BalanceView> BalanceViews
        {
            get { return balanceViews; }
            set
            {
                if(value!= null)
                {
                    balanceViews = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private Visibility visibilityShowPanelBalance = Visibility.Hidden;
        public Visibility VisibilityShowPanelBalance
        {
            get { return visibilityShowPanelBalance; }
            set
            {
                visibilityShowPanelBalance = value;
                base.NotifyPropertyChanged();
            }
        }

        private RelayCommand showPanelBalanceCommand;
        public RelayCommand ShowPanelBalanceCommand
        {
            get
            {
                return showPanelBalanceCommand ?? new RelayCommand((object o) =>
                {
                    VisibilityShowPanelBalance = Visibility.Visible;
                    if(selectedKey != null)
                    {
                        var balances = balanceRepository.Get(selectedKey.PublicKey);
                        var balancesView = new List<BalanceView>();
                        foreach (var balance in balances)
                        {
                            balancesView.Add(new BalanceView()
                            {
                                Asset = balance.Asset,
                                Free = balance.Free,
                                Locked = balance.Locked
                            });
                        }
                        BalanceViews = balancesView;
                    }
                    else
                    {
                        BalanceViews = new List<BalanceView>();
                    }
                });
            }
        }

        private RelayCommand hidePanelBalanceCommand;
        public RelayCommand HidePanelBalanceCommand
        {
            get
            {
                return hidePanelBalanceCommand ?? new RelayCommand((object o) =>
                {
                    VisibilityShowPanelBalance = Visibility.Hidden;
                    BalanceViews = new List<BalanceView>();
                });
            }
        }
        #endregion

        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                base.NotifyPropertyChanged();
            }
        }

        private string publicKey;
        public string PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                base.NotifyPropertyChanged();
            }
        }

        private string secretKey;
        public string SecretKey
        {
            get { return secretKey; }
            set
            {
                secretKey = value;
                base.NotifyPropertyChanged();
            }
        }
    }
}
