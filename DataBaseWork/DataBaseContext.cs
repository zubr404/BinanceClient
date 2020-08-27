using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork
{
    public class DataBaseContext : DbContext
    {
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeConfiguration> TradeConfigurations { get; set; }
        public DbSet<APIKey> APIKeys { get; set; }
        public DbSet<TradeHistory> TradeHistories { get; set; }
        public DbSet<ConnectedPair> ConnectedPairs { get; set; }
        public DbSet<Balance> Balances { get; set; }

        public DataBaseContext()
        {
            try
            {
                //Database.EnsureDeleted();
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                
            }
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BinanceDB;Username=postgres;Password=123456");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>() // отношение по альтернативному ключу
                .HasOne(b => b.APIKey)
                .WithMany(a => a.Balances)
                .HasForeignKey(b => b.FK_PublicKey)
                .HasPrincipalKey(a => a.PublicKey);
            modelBuilder.Entity<Trade>() // отношение по альтернативному ключу
                .HasOne(b => b.APIKey)
                .WithMany(a => a.Trades)
                .HasForeignKey(b => b.FK_PublicKey)
                .HasPrincipalKey(a => a.PublicKey);
        }
    }
}
