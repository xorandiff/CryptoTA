using CryptoTA.Models;
using System.Data.Entity;

namespace CryptoTA
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<Market> Markets { get; set; }
        public DbSet<TradingPair> TradingPairs { get; set; }
    }
}
