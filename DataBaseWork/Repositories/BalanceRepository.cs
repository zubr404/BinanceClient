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

        public Balance Update(Balance item) 
        {
            var balance = db.Balances.FirstOrDefault(x => x.APIKeyID == item.APIKeyID && x.Asset == item.Asset);
            if(balance == null)
            {
                balance = db.Balances.Add(item).Entity;
                Save();
            }
            else
            {
                balance.Free = item.Free;
                balance.Locked = item.Locked;
            }
            return balance;
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
