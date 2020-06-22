using DataBaseWork.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataBaseWork.Repositories
{
    public class UserRepository
    {
        readonly DataBaseContext db;
        public UserRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool Exists(string login)
        {
            if (!string.IsNullOrWhiteSpace(login))
            {
                return db.Users.AsNoTracking().Any(x => x.Login == login);
            }
            else
            {
                throw new ArgumentException("Параметр не содержит допустимого значения.", "name");
            }
        }

        public IEnumerable<User> Get()
        {
            return db.Users;
        }

        public User Get(int id)
        {
            return db.Users.FirstOrDefault(u => u.ID == id);
        }

        public User Create(User item)
        {
            if (!Exists(item.Login))
            {
                try
                {
                    var user = db.Users.Add(item);
                    Save();
                    return user.Entity;
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new ArgumentException("Пользователь с таким логином уже существует.", "item");
            }
        }

        public User Update(User item)
        {
            if (!Exists(item.Login))
            {
                var user = db.Users.FirstOrDefault(x => x.ID == item.ID);
                if (user != null)
                {
                    user.Login = item.Login;
                    Save();
                    return user;
                }
                return null;
            }
            else
            {
                throw new ArgumentException("Пользователь с таким логином уже существует.", "item");
            }
        }

        public bool Delete(int id)
        {
            var user = db.Users.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                db.Users.Remove(user);
                Save();
                return true;
            }
            return false;
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
