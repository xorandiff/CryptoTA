using CryptoTA.Apis;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoTA.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Credentials> Credentials => Set<Credentials>();
        public DbSet<Market> Markets => Set<Market>();
        public DbSet<TradingPair> TradingPairs => Set<TradingPair>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Tick> Ticks => Set<Tick>();
        public DbSet<Settings> Settings => Set<Settings>();
        public DbSet<TimeInterval> TimeIntervals => Set<TimeInterval>();
        public DbSet<Strategy> Strategies => Set<Strategy>();
        public DbSet<StrategyCategory> StrategyCategories => Set<StrategyCategory>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=CryptoTA",
                providerOptions => { providerOptions.EnableRetryOnFailure(); });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<StrategyCategory>().HasData(
                new() { StrategyCategoryId = 1, Name = "Rapid (less than an hour)" },
                new() { StrategyCategoryId = 2, Name = "Short (less than a day)" },
                new() { StrategyCategoryId = 3, Name = "Medium (less than 3 days)" },
                new() { StrategyCategoryId = 4, Name = "Long (less than a week)" }
            );

            _ = modelBuilder.Entity<TimeInterval>().HasData(
                new() { TimeIntervalId = 1, Name = "1 day", Seconds = 86400, IsIndicatorInterval = false },
                new() { TimeIntervalId = 2, Name = "3 days", Seconds = 86400 * 3, IsIndicatorInterval = false },
                new() { TimeIntervalId = 3, Name = "1 week", Seconds = 86400 * 7, IsIndicatorInterval = false },
                new() { TimeIntervalId = 4, Name = "2 weeks", Seconds = 86400 * 14, IsIndicatorInterval = false },
                new() { TimeIntervalId = 5, Name = "1 month", Seconds = 86400 * 31, IsIndicatorInterval = false },
                new() { TimeIntervalId = 6, Name = "3 months", Seconds = 86400 * 31 * 3, IsIndicatorInterval = false },
                new() { TimeIntervalId = 7, Name = "6 months", Seconds = 86400 * 31 * 6, IsIndicatorInterval = false },
                new() { TimeIntervalId = 8, Name = "1 year", Seconds = 86400 * 31 * 12, IsIndicatorInterval = false },
                new() { TimeIntervalId = 9, Name = "5 years", Seconds = 86400 * 31 * 12 * 5, IsIndicatorInterval = false },

                new() { TimeIntervalId = 10, Name = "1 minute", Seconds = 60, IsIndicatorInterval = true },
                new() { TimeIntervalId = 11, Name = "5 minutes", Seconds = 60 * 5, IsIndicatorInterval = true },
                new() { TimeIntervalId = 12, Name = "15 minutes", Seconds = 60 * 15, IsIndicatorInterval = true },
                new() { TimeIntervalId = 13, Name = "30 minutes", Seconds = 60 * 30, IsIndicatorInterval = true },
                new() { TimeIntervalId = 14, Name = "1 hour", Seconds = 60 * 60, IsIndicatorInterval = true },
                new() { TimeIntervalId = 15, Name = "2 hours", Seconds = 60 * 60 * 2, IsIndicatorInterval = true },
                new() { TimeIntervalId = 16, Name = "4 hours", Seconds = 60 * 60 * 4, IsIndicatorInterval = true },
                new() { TimeIntervalId = 17, Name = "1 day", Seconds = 60 * 60 * 24, IsIndicatorInterval = true },
                new() { TimeIntervalId = 18, Name = "1 week", Seconds = 60 * 60 * 24 * 7, IsIndicatorInterval = true },
                new() { TimeIntervalId = 19, Name = "1 month", Seconds = 60 * 60 * 24 * 31, IsIndicatorInterval = true }
            );
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
