using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoTA.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<TradingPair> TradingPairs { get; set; }
        public DbSet<Tick> Ticks { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<TimeInterval> TimeIntervals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=CryptoTA",
                providerOptions => { providerOptions.EnableRetryOnFailure(); });
        }
    }
}
