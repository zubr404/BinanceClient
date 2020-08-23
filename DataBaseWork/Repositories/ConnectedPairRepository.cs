using DataBaseWork.Models;
using System;
using System.Collections.Generic;
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

        public IEnumerable<ConnectedPair> Get()
        {
            return db.ConnectedPairs;
        }
    }
}
