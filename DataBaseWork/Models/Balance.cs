using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class Balance
    {
        public int ID { get; set; }
        public string FK_PublicKey { get; set; } // связь должна быть по публичному ключу
        public string Asset { get; set; }
        public string Free { get; set; }
        public string Locked { get; set; }
        public APIKey APIKey { get; set; }
    }
}
