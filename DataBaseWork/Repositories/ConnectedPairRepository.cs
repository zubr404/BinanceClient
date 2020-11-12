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
        public bool Exists(ConnectedPair item)
        {
            using (var db = new DataBaseContext())
            {
                return db.ConnectedPairs.Any(x => x.AltCoin == item.AltCoin && x.MainCoin == item.MainCoin);
            }
        }

        public IEnumerable<ConnectedPair> Get()
        {
            using (var db = new DataBaseContext())
            {
                return db.ConnectedPairs.AsNoTracking().ToArray();
            }
        }

        public IEnumerable<ConnectedPair> GetActive()
        {
            using (var db = new DataBaseContext())
            {
                return db.ConnectedPairs.AsNoTracking().Where(x => x.Active).ToArray();
            }
        }

        public ConnectedPair Create(ConnectedPair item)
        {
            using (var db = new DataBaseContext())
            {
                ConnectedPair result = null;
                if (!Exists(item))
                {
                    result = db.ConnectedPairs.Add(item).Entity;
                }
                db.SaveChanges();
                return result;
            }
        }

        public void Create(IEnumerable<ConnectedPair> pairs)
        {
            using (var db = new DataBaseContext())
            {
                db.ConnectedPairs.AddRange(pairs);
                db.SaveChanges();
            }
        }

        public ConnectedPair Update(ConnectedPair item)
        {
            using (var db = new DataBaseContext())
            {
                var result = db.ConnectedPairs.FirstOrDefault(x => x.MainCoin == item.MainCoin && x.AltCoin == item.AltCoin);
                if (result != null)
                {
                    result.MainCoin = item.MainCoin;
                    result.AltCoin = item.AltCoin;
                    result.Active = item.Active;
                }
                db.SaveChanges();
                return result;
            }
        }

        public ConnectedPair CreateOrUpdate(ConnectedPair item)
        {
            using (var db = new DataBaseContext())
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
                db.SaveChanges();
                return result;
            }

        }
    }
}
