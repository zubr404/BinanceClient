using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;

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
        public DbSet<StopLimitOrder> StopLimitOrders { get; set; }
        public DbSet<TakeProfitOrder> TakeProfitOrders { get; set; }

        public DataBaseContext()
        {
            try
            {
                //Database.EnsureDeleted();
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BinanceDB;Username=postgres;Password=1234567");
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
            modelBuilder.Entity<StopLimitOrder>() // отношение по альтернативному ключу
                .HasOne(b => b.APIKey)
                .WithMany(a => a.StopLimitOrders)
                .HasForeignKey(b => b.FK_PublicKey)
                .HasPrincipalKey(a => a.PublicKey);
            modelBuilder.Entity<TakeProfitOrder>() // отношение по альтернативному ключу
                .HasOne(b => b.APIKey)
                .WithMany(a => a.TakeProfitOrders)
                .HasForeignKey(b => b.FK_PublicKey)
                .HasPrincipalKey(a => a.PublicKey);

            //--insert into public."ConnectedPairs"("MainCoin","AltCoin","Active") values
            //-- ('BTC', 'USDT', true),
            //-- ('ETH', 'USDT', true)

            //modelBuilder.Entity<ConnectedPair>().HasData(
            //new ConnectedPair[]
            //{
            //    new ConnectedPair { ID = 1, MainCoin = "BTC", AltCoin = "ETH", Active = true },
            //    new ConnectedPair { ID = 2, MainCoin = "BTC", AltCoin = "USDT", Active = true },
            //    new ConnectedPair { ID = 2, MainCoin = "ETH", AltCoin = "USDT", Active = true }
            //});
        }
    }
}
