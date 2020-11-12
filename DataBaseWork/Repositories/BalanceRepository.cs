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
        public IEnumerable<Balance> Get()
        {
            using (var db = new DataBaseContext())
            {
                return db.Balances.AsNoTracking();
            }
        }

        public IEnumerable<Balance> Get(string publicKey)
        {
            using (var db = new DataBaseContext())
            {
                return db.Balances.AsNoTracking().Where(x => x.FK_PublicKey == publicKey).ToArray();
            }
        }

        public Balance Get(string publicKey, string asset)
        {
            using (var db = new DataBaseContext())
            {
                return db.Balances.AsNoTracking().FirstOrDefault(x => x.FK_PublicKey == publicKey && x.Asset == asset);
            }
        }

        public Balance Update(Balance item) 
        {
            using (var db = new DataBaseContext())
            {
                var balance = db.Balances.FirstOrDefault(x => x.FK_PublicKey == item.FK_PublicKey && x.Asset == item.Asset);
                if (balance == null)
                {
                    balance = db.Balances.Add(item).Entity;
                    db.SaveChanges();
                }
                else
                {
                    balance.Free = item.Free;
                    balance.Locked = item.Locked;
                    db.SaveChanges();
                }
                return balance;
            }
            
        }
    }
}
