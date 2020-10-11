using System;
using System.Collections.Generic;
using System.Text;

namespace Algoritms.BackTest
{
    public class BackTestConfiguration
    {
        public GeneralSetting GeneralSetting { get; set; }
        public string MainCoin { get; set; }
        public string AltCoin { get; set; }
        public string Strategy { get; set; }
        public bool Margin { get; set; }
        public double OrderIndent { get; set; }
        public double OrderDeposit { get; set; }
        public double FirstStep { get; set; }
        public double OrderStepPlus { get; set; }
        public double Martingale { get; set; }
        public double DepositLimit { get; set; }
        public double OrderReload { get; set; }
        public double Loss { get; set; }
        public double Profit { get; set; }
        public double IndentExtremum { get; set; }
        public double ProtectiveSpread { get; set; }

        public BackTestConfiguration()
        {
            GeneralSetting = new GeneralSetting();
        }
    }

    public class GeneralSetting
    {
        /// <summary>
        /// Общий размер счета в валюте котировки
        /// </summary>
        public double Deposit { get; private set; }
        /// <summary>
        /// Депозит основной валюты
        /// </summary>
        public double DepositAsset { get; set; }
        /// <summary>
        /// Депозит валюты котировки
        /// </summary>
        public double DepositQuote { get; set; }
        /// <summary>
        /// Начальная точка исторических данных (timestamp)
        /// </summary>
        public long StartTime { get; set; }
        /// <summary>
        /// Конечная точка исторических данных (timestamp)
        /// </summary>
        public long StopTime { get; set; }

        public void SetDeposit(double price)
        {
            Deposit = DepositQuote + DepositAsset * price;
        }
    }
}
