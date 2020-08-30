using DataBaseWork.Models;
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

        public IEnumerable<ConnectedPair> GetActive()
        {
            return db.ConnectedPairs.Where(x=>x.Active);
        }
    }
}
