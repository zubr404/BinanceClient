using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class APIKey
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public bool Active { get; set; }
    }
}
