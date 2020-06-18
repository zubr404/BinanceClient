using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork
{
    public class DataBaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DataBaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BinanceDB;Username=postgres;Password=123456");
            // Server=localhost;User Id=postgres;Port=5432;Password=123456;Database=GalaxyDB;
        }
    }
}
