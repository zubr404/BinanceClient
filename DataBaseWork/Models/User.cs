using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<APIKey> APIKeys { get; set; }

        public User()
        {
            APIKeys = new List<APIKey>();
        }
    }
}
