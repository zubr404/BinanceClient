using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class Balance
    {
        public int ID { get; set; }
        public int APIKeyID { get; set; }
        public string Asset { get; set; }
        public string Free { get; set; }
        public string Locked { get; set; }
    }
}
