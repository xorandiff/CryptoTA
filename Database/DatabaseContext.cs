using CryptoTA.Database.Models;
using System.Data.Entity;

namespace CryptoTA.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<TradingPair> TradingPairs { get; set; }
        public DbSet<Tick> Ticks { get; set; }
        public DbSet<Settings> Configuration { get; set; }
        public DbSet<TimeInterval> TimeIntervals { get; set; }
    }
}
