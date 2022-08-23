using CryptoTA.Apis;
using CryptoTA.Database.Models;
using LiveCharts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

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

        public Market GetMarketFromSettings()
        {
            var settings = Settings.First();
            var dbTradingPair = TradingPairs.Find(settings.TradingPairId);

            if (dbTradingPair != null)
            {
                var dbMarket = Markets.Find(dbTradingPair.MarketId);

                if (dbMarket != null)
                {
                    return dbMarket;
                }
                else
                {
                    throw new Exception("Stored trading pair in Settings table has no corresponding Market assigned in database.");
                }
            }
            else
            {
                throw new Exception("Couldn't find stored trading pair in Settings table.");
            }
        }

        public TradingPair GetTradingPairFromSettings()
        {
            var settings = Settings.First();
            var dbTradingPair = TradingPairs.Find(settings.TradingPairId);

            if (dbTradingPair != null)
            {
                return dbTradingPair;
            }
            else
            {
                throw new Exception("Couldn't find stored trading pair in Settings table.");
            }
        }

        public TimeInterval GetTimeIntervalFromSettings()
        {
            var settings = Settings.Include("TimeInterval").First();
            if (settings.TimeInterval is TimeInterval dbTimeInterval)
            {
                return dbTimeInterval;
            }
            else
            {
                throw new Exception("Couldn't find stored time interval in Settings table.");
            }
        }

        public async Task<List<Tick>> GetTicks(int tradingPairId, DateTime startDate)
        {
            if (await TradingPairs.Where(tp => tp.TradingPairId == tradingPairId).Include(tp => tp.Ticks).Include(tp => tp.Market).FirstOrDefaultAsync() is TradingPair dbTradingPair)
            {
                MarketApis marketApis = new();
                marketApis.setActiveApiByName(dbTradingPair.Market!.Name);

                if (!dbTradingPair.Ticks.Any())
                {
                    var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                    dbTradingPair.Ticks.AddRange(ticks);

                    await SaveChangesAsync();
                }

                var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;

                if (!dbTradingPair.Ticks.Any(tick => tick.Date < startDate))
                {
                    var oldestStoredDate = dbTradingPair.Ticks.Select(tick => tick.Date).Min();

                    while (oldestStoredDate >= startDate)
                    {
                        oldestStoredDate = oldestStoredDate.AddSeconds(-maxTimeInterval);
                        var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                        dbTradingPair.Ticks.AddRange(ticks);
                    }

                    await SaveChangesAsync();
                }

                var currentDateLower = DateTime.Now.AddSeconds(-marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                var newestStoredDate = dbTradingPair.Ticks.Select(tick => tick.Date).Max();

                while (newestStoredDate < currentDateLower)
                {
                    var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, newestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                    dbTradingPair.Ticks.AddRange(ticks);
                    newestStoredDate = newestStoredDate.AddSeconds(maxTimeInterval);
                }

                await SaveChangesAsync();

                return dbTradingPair.Ticks.Where(tick => tick.Date >= startDate).OrderByDescending(t => t.Date).ToList();
            }

            return new List<Tick>();
        }
    }
}
