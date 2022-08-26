using CryptoTA.Apis;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<List<Tick>> GetTicks(int tradingPairId, DateTime? tickStartDate = null, uint ticksCountLimit = 0)
        {
            if (await TradingPairs.Where(tp => tp.TradingPairId == tradingPairId).Include(tp => tp.Market).AsSplitQuery().FirstOrDefaultAsync() is TradingPair dbTradingPair)
            {
                try
                {
                    MarketApis marketApis = new();
                    marketApis.setActiveApiByName(dbTradingPair.Market!.Name);
                    var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
                    var smallestMarketRequestInterval = marketApis.ActiveMarketApi.OhlcTimeIntervals.Min();
                    var currentDateLower = DateTime.Now.AddSeconds(-smallestMarketRequestInterval);

                    var startDate = DateTime.Now.AddSeconds(-maxTimeInterval);
                    if (tickStartDate != null)
                    {
                        startDate = tickStartDate.Value;
                    }

                    IQueryable<Tick> tradingPairTicksQuery = Ticks.Where(tick => tick.TradingPairId == tradingPairId);

                    if (!tradingPairTicksQuery.Any(tick => tick.Date <= startDate))
                    {
                        var oldestStoredDate = tradingPairTicksQuery.Any() ? tradingPairTicksQuery.Select(tick => tick.Date).Min() : DateTime.Now;

                        while (oldestStoredDate >= startDate.AddSeconds(-smallestMarketRequestInterval))
                        {
                            oldestStoredDate = oldestStoredDate.AddSeconds(-maxTimeInterval);
                            var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, oldestStoredDate, smallestMarketRequestInterval);

                            foreach (var tick in ticks) tick.TradingPairId = tradingPairId;
                            Ticks.AddRange(ticks);
                        }

                        await SaveChangesAsync();
                    }

                    var newestStoredDate = tradingPairTicksQuery.Select(tick => tick.Date).Max();
                    while (newestStoredDate < currentDateLower)
                    {
                        newestStoredDate = newestStoredDate.AddSeconds(maxTimeInterval);
                        var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, newestStoredDate, smallestMarketRequestInterval);
                        
                        foreach (var tick in ticks) tick.TradingPairId = tradingPairId;
                        Ticks.AddRange(ticks);
                    }

                    await SaveChangesAsync();

                    IQueryable<Tick> ticksQuery = tradingPairTicksQuery.AsNoTracking().OrderBy(t => t.Date);
                    
                    ticksQuery = ticksQuery.Where(tick => tick.Date >= startDate);
                    
                    var ticksCount = ticksQuery.Count();
                    if (ticksCountLimit > 0 && ticksCount > ticksCountLimit)
                    {
                        var indexDivisor = (int) (ticksCount / ticksCountLimit);
                        return ticksQuery.AsEnumerable().Where((tick, i) => i % indexDivisor == 0).ToList();
                    }

                    return await ticksQuery.ToListAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            return new List<Tick>();
        }
    }
}
