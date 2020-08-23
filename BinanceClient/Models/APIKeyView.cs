using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;

namespace BinanceClient.Models
{
    public class APIKeyView : PropertyChangedBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    name = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private string publicKey;
        public string PublicKey
        {
            get { return publicKey; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    publicKey = value;
                    base.NotifyPropertyChanged();
                }
            }
        }

        private string secretKey;
        public string SecretKey
        {
            get { return secretKey; }
            set
            {
                if(!string.IsNullOrWhiteSpace(value))
                {
                    secretKey = value;
                    base.NotifyPropertyChanged();
                }
            }
        }
    }
}
