using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class ConnectedPairRepository
    {
        readonly DataBaseContext db;
        public ConnectedPairRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool Exists(ConnectedPair item)
        {
            return db.ConnectedPairs.Any(x => x.AltCoin == item.AltCoin && x.MainCoin == item.MainCoin);
        }

        public IEnumerable<ConnectedPair> Get()
        {
            return db.ConnectedPairs.AsNoTracking();
        }

        public IEnumerable<ConnectedPair> GetActive()
        {
            return db.ConnectedPairs.AsNoTracking().Where(x=>x.Active);
        }

        public ConnectedPair Create(ConnectedPair item)
        {
            ConnectedPair result = null;
            if (!Exists(item))
            {
                result = db.ConnectedPairs.Add(item).Entity;
            }
            Save();
            return result;
        }

        public void Create(IEnumerable<ConnectedPair> pairs)
        {
            db.ConnectedPairs.AddRange(pairs);
            Save();
        }

        public ConnectedPair Update(ConnectedPair item)
        {
            var result = db.ConnectedPairs.FirstOrDefault(x => x.MainCoin == item.MainCoin && x.AltCoin == item.AltCoin);
            if (result != null)
            {
                result.MainCoin = item.MainCoin;
                result.AltCoin = item.AltCoin;
                result.Active = item.Active;
            }
            Save();
            return result;
        }

        public ConnectedPair CreateOrUpdate(ConnectedPair item)
        {
            var result = db.ConnectedPairs.FirstOrDefault(x => x.MainCoin == item.MainCoin && x.AltCoin == item.AltCoin);
            if (result != null)
            {
                result.MainCoin = item.MainCoin;
                result.AltCoin = item.AltCoin;
                result.Active = item.Active;
            }
            else
            {
                result = db.ConnectedPairs.Add(item).Entity;
            }
            Save();
            return result;
        }

        private void Save()
        {
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
