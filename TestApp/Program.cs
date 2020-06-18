using DataBaseWork;
using DataBaseWork.Models;
using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var db = new DataBaseContext())
            {
                var user = new User()
                {
                    Login = "login",
                    Password = "password"
                };

                db.Users.Add(user);
                db.SaveChanges();
            }
        }
    }
}
