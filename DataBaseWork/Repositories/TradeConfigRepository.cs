using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class TradeConfigRepository
    {
        readonly DataBaseContext db;
        public TradeConfigRepository(DataBaseContext db)
        {
            this.db = db;
        }
    }
}
