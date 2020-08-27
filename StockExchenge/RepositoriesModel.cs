using DataBaseWork.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockExchenge
{
    /// <summary>
    /// Главным образом для UserStream.
    /// </summary>
    public class RepositoriesModel
    {
        public APIKeyRepository APIKeyRepository { get; set; }
        public BalanceRepository BalanceRepository { get; set; }
    }
}
