using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class BalanceRepository
    {
        readonly DataBaseContext db;
        public BalanceRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public IEnumerable<Balance> Get()
        {
            return db.Balances.AsNoTracking();
        }

        public Balance Get(string publicKey, string asset)
        {
            return db.Balances.AsNoTracking().FirstOrDefault(x => x.FK_PublicKey == publicKey && x.Asset == asset);
        }

        public Balance Update(Balance item) 
        {
            var balance = db.Balances.FirstOrDefault(x => x.FK_PublicKey == item.FK_PublicKey && x.Asset == item.Asset);
            if(balance == null)
            {
                balance = db.Balances.Add(item).Entity;
                Save();
            }
            else
            {
                balance.Free = item.Free;
                balance.Locked = item.Locked;
                Save();
            }
            return balance;
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
