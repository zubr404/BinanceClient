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
        readonly APIKeyRepository repository;
        public APIKeyView APIKeyView { get; private set; }
        public KeyPanelScrin1(APIKeyRepository repository)
        {
            this.repository = repository;
            APIKeyView = new APIKeyView();
        }

        private double height = 0;
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                base.NotifyPropertyChanged();
            }
        }

        private double width = 0;
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                base.NotifyPropertyChanged();
            }
        }

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

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? new RelayCommand((object o) =>
                {
                    var key = new APIKey()
                    {
                        Name = APIKeyView.Name,
                        PublicKey = APIKeyView.PublicKey,
                        SecretKey = APIKeyView.SecretKey
                    };
                    try
                    {
                        repository.Update(key);
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    ClosePanel();
                });
            }
        }

        private RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return cancelCommand ?? new RelayCommand((object o) =>
                {
                    ClosePanel();
                });
            }
        }

        public void ClosePanel()
        {
            Height = 0;
            Width = 0;
        }

        public void OpenPanel()
        {
            Height = 200;
            Width = 400;
        }
    }
}
