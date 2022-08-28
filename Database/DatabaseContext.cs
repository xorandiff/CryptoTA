using CryptoTA.Apis;
using CryptoTA.Database.Models;
using CryptoTA.UserControls;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoTA.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Credentials> Credentials => Set<Credentials>();
        public DbSet<Market> Markets => Set<Market>();
        public DbSet<TradingPair> TradingPairs => Set<TradingPair>();
        public DbSet<Tick> Ticks => Set<Tick>();
        public DbSet<Settings> Settings => Set<Settings>();
        public DbSet<TimeInterval> TimeIntervals => Set<TimeInterval>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=CryptoTA",
                providerOptions => { providerOptions.EnableRetryOnFailure(); });
        }

        public async Task<Market> GetMarketFromSettings()
        {
            if (await Markets.FindAsync((await GetTradingPairFromSettings()).MarketId) is Market dbMarket)
            {
                return dbMarket;
            }
            else
            {
                throw new Exception("Stored trading pair in Settings table has no corresponding Market assigned in database.");
            }
        }

        public async Task<TradingPair> GetTradingPairFromSettings()
        {
            if (await Settings.FirstOrDefaultAsync() is Settings settings && await TradingPairs.FindAsync(settings.TradingPairId) is TradingPair dbTradingPair)
            {
                return dbTradingPair;
            }
            else
            {
                throw new Exception("Couldn't find stored trading pair in Settings table.");
            }
        }

        public async Task<TimeInterval> GetTimeIntervalFromSettings(bool isIndicatorInterval)
        {
            var settings = Settings.First();
            var dbTimeIntervalId = isIndicatorInterval ? settings.TimeIntervalIdIndicators : settings.TimeIntervalIdChart;
            if (await TimeIntervals.Where(ti => ti.TimeIntervalId == dbTimeIntervalId).FirstOrDefaultAsync() is TimeInterval dbTimeInterval)
            {
                return dbTimeInterval;
            }
            else
            {
                throw new Exception("Couldn't find stored time interval in Settings table.");
            }
        }
    }
}
