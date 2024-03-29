﻿using CryptoTA.Apis;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoTA.Database
{
    public class DatabaseModel
    {
        private readonly MarketApis marketApis;

        public BackgroundWorker worker;

        private class GetTicksData
        {
            public int TradingPairId { get; set; }
            public DateTime? TickStartDate { get; set; }
            public uint TicksCountLimit { get; set; }
        }

        public DatabaseModel()
        {
            marketApis = new();
            worker = new();
            worker.DoWork += Worker_DoWork;
            worker.WorkerReportsProgress = true;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (sender == null || e.Argument is not GetTicksData data)
            {
                e.Cancel = true;
                return;
            }

            // Get arguments from BackgroundWorker event
            using var db = new DatabaseContext();
            int tradingPairId = data.TradingPairId;
            DateTime? tickStartDate = data.TickStartDate;
            uint ticksCountLimit = data.TicksCountLimit;

            // Get TradingPair object from database with included Market object
            if (db.TradingPairs.Where(tp => tp.TradingPairId == tradingPairId).Include(tp => tp.Market).FirstOrDefault() is not TradingPair dbTradingPair)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                marketApis.SetActiveApiByName(dbTradingPair.Market!.Name);

                // Get maximum time interval (with maximum data density, in seconds) for single query,
                // available in currently used API
                var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;

                // Smallest time intervals between ticks (in seconds) available in currently used API, 
                // it's used for filling newest ticks up to current date.
                //
                // Most of the time, last API request with newest ticks will be less than maxTimeInterval,
                // so we want to make sure we downlad all available data up to current date
                var minTickInterval = marketApis.ActiveMarketApi.OhlcTimeIntervals.Min();

                // Maxmimal date for which requests will be fired if there is no tick greater than date 
                // in the database
                var maxDate = DateTime.Now.AddSeconds(-minTickInterval);

                // Initialize progress tracking variables
                var downloadRequestsCount = 0;
                var completedRequests = 0;
                var reportProgress = (int completed, int total) =>
                {
                    if (total > 0)
                    {
                        worker.ReportProgress((int)(completed * 100d / total));
                    }
                };

                // If no start date is specified, then we set it to date for only one request
                DateTime startDate = tickStartDate ?? DateTime.Now.AddSeconds(-maxTimeInterval);

                // Base query for ticks belonging to current TradingPair
                IQueryable<Tick> tradingPairTicksQuery = db.Ticks.Where(tick => tick.TradingPairId == tradingPairId);

                var checkDate = new DateTime(2000, 1, 1);
                if (db.Ticks.Any(tick => tick.Date < checkDate))
                {
                    db.RemoveRange(db.Ticks.Where(tick => tick.Date < checkDate).ToArray());
                    db.SaveChanges();
                }

                // We check if there are any stored ticks in database
                if (tradingPairTicksQuery.Any())
                {
                    //If there are some ticks, then we find newest and oldest stored dates
                    var newestStoredDate = tradingPairTicksQuery.Select(tick => tick.Date).Max();
                    var oldestStoredDate = tradingPairTicksQuery.Select(tick => tick.Date).Min();

                    int requestCountBefore = 0;
                    int requestCountAfter = 0;

                    if (oldestStoredDate > startDate)
                    { 
                        requestCountBefore = (int)Math.Ceiling((oldestStoredDate - startDate).TotalSeconds / maxTimeInterval);
                    }
                    if (newestStoredDate < maxDate)
                    {
                        requestCountAfter = (int)Math.Ceiling((maxDate - newestStoredDate).TotalSeconds / maxTimeInterval);
                    }

                    downloadRequestsCount = requestCountBefore + requestCountAfter;

                    for (int i = 0; i < requestCountBefore; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, startDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);

                        completedRequests++;
                        reportProgress(completedRequests, downloadRequestsCount);
                    }

                    for (int i = 0; i < requestCountAfter; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, newestStoredDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);

                        completedRequests++;
                        reportProgress(completedRequests, downloadRequestsCount);
                    }
                }
                else
                {
                    downloadRequestsCount = (int)Math.Ceiling((maxDate - startDate).TotalSeconds / maxTimeInterval);

                    for (int i = 0; i < downloadRequestsCount; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, startDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);

                        completedRequests++;
                        reportProgress(completedRequests, downloadRequestsCount);
                    }
                }

                db.SaveChanges();

                IQueryable<Tick> ticksQuery = tradingPairTicksQuery.AsNoTracking().OrderBy(t => t.Date);

                ticksQuery = ticksQuery.Where(tick => tick.Date >= startDate);

                var ticksCount = ticksQuery.Count();
                if (ticksCountLimit > 0 && ticksCount > ticksCountLimit)
                {
                    var indexDivisor = (int)(ticksCount / ticksCountLimit);
                    e.Result = ticksQuery.AsEnumerable().Where((tick, i) => i % indexDivisor == 0).ToList();
                }
                else
                {
                    e.Result = ticksQuery.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void GetTicks(int tradingPairId, DateTime? tickStartDate = null, uint ticksCountLimit = 0)
        {
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync(new GetTicksData
                {
                    TradingPairId = tradingPairId,
                    TickStartDate = tickStartDate,
                    TicksCountLimit = ticksCountLimit
                });
            }
        }

        public List<Tick> GetTicksBlocking(int tradingPairId, DateTime? tickStartDate = null, uint ticksCountLimit = 0)
        {
            List<Tick> result = new();

            try
            {
                using DatabaseContext db = new();

                // Get TradingPair object from database with included Market object
                if (db.TradingPairs.Where(tp => tp.TradingPairId == tradingPairId).Include(tp => tp.Market).FirstOrDefault() is not TradingPair dbTradingPair)
                {
                    return result;
                }

                marketApis.SetActiveApiByName(dbTradingPair.Market!.Name);

                // Get maximum time interval (with maximum data density, in seconds) for single query,
                // available in currently used API
                var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;

                // Smallest time intervals between ticks (in seconds) available in currently used API, 
                // it's used for filling newest ticks up to current date.
                //
                // Most of the time, last API request with newest ticks will be less than maxTimeInterval,
                // so we want to make sure we downlad all available data up to current date
                var minTickInterval = marketApis.ActiveMarketApi.OhlcTimeIntervals.Min();

                // Maxmimal date for which requests will be fired if there is no tick greater than date 
                // in the database
                var maxDate = DateTime.Now.AddSeconds(-minTickInterval);

                int downloadRequestsCount = 0;

                // If no start date is specified, then we set it to date for only one request
                DateTime startDate = tickStartDate ?? DateTime.Now.AddSeconds(-maxTimeInterval);

                // Base query for ticks belonging to current TradingPair
                IQueryable<Tick> tradingPairTicksQuery = db.Ticks.Where(tick => tick.TradingPairId == tradingPairId);

                var checkDate = new DateTime(2000, 1, 1);
                if (db.Ticks.Any(tick => tick.Date < checkDate))
                {
                    db.RemoveRange(db.Ticks.Where(tick => tick.Date < checkDate).ToArray());
                    db.SaveChanges();
                }

                // We check if there are any stored ticks in database
                if (tradingPairTicksQuery.Any())
                {
                    //If there are some ticks, then we find newest and oldest stored dates
                    var newestStoredDate = tradingPairTicksQuery.Select(tick => tick.Date).Max();
                    var oldestStoredDate = tradingPairTicksQuery.Select(tick => tick.Date).Min();

                    int requestCountBefore = 0;
                    int requestCountAfter = 0;

                    if (oldestStoredDate > startDate)
                    {
                        requestCountBefore = (int)Math.Ceiling((oldestStoredDate - startDate).TotalSeconds / maxTimeInterval);
                    }
                    if (newestStoredDate < maxDate)
                    {
                        requestCountAfter = (int)Math.Ceiling((maxDate - newestStoredDate).TotalSeconds / maxTimeInterval);
                    }

                    downloadRequestsCount = requestCountBefore + requestCountAfter;

                    for (int i = 0; i < requestCountBefore; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, startDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);
                    }

                    for (int i = 0; i < requestCountAfter; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, newestStoredDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);
                    }
                }
                else
                {
                    downloadRequestsCount = (int)Math.Ceiling((maxDate - startDate).TotalSeconds / maxTimeInterval);

                    for (int i = 0; i < downloadRequestsCount; i++)
                    {
                        var ticks = marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, startDate.AddSeconds(maxTimeInterval * i), minTickInterval);

                        foreach (var tick in ticks)
                        {
                            tick.TradingPairId = tradingPairId;
                        }
                        db.Ticks.AddRange(ticks);
                    }
                }

                _ = db.SaveChanges();

                IQueryable<Tick> ticksQuery = tradingPairTicksQuery.AsNoTracking().OrderBy(t => t.Date);

                ticksQuery = ticksQuery.Where(tick => tick.Date >= startDate);

                var ticksCount = ticksQuery.Count();
                if (ticksCountLimit > 0 && ticksCount > ticksCountLimit)
                {
                    var indexDivisor = (int)(ticksCount / ticksCountLimit);
                    return ticksQuery.AsEnumerable().Where((tick, i) => i % indexDivisor == 0).ToList();
                }
                else
                {
                    return ticksQuery.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;
        }

        public async Task<Tick?> GetTick(TradingPair tradingPair)
        {
            using DatabaseContext db = new();

            if (await db.TradingPairs.Where(tp => tp.TradingPairId == tradingPair.TradingPairId).Include(tp => tp.Market).FirstOrDefaultAsync() is not TradingPair dbTradingPair)
            {
                return null;
            }

            marketApis.SetActiveApiByName(dbTradingPair.Market!.Name);

            return await marketApis.ActiveMarketApi.GetTickAsync(tradingPair);
        }

        public Tick? GetTickBlocking(TradingPair tradingPair)
        {
            using DatabaseContext db = new();

            if (db.TradingPairs.Where(tp => tp.TradingPairId == tradingPair.TradingPairId).Include(tp => tp.Market).FirstOrDefault() is not TradingPair dbTradingPair)
            {
                return null;
            }

            marketApis.SetActiveApiByName(dbTradingPair.Market!.Name);

            return marketApis.ActiveMarketApi.GetTick(tradingPair);
        }

        public static List<Tick> GetTickPeriods(List<Tick> ticks, long periodSecondsInterval, int periodsCount)
        {
            var startDate = ticks.Last().Date;

            List<Tick> result = new();

            for (int i = 0; i < periodsCount; i++)
            {
                startDate = startDate.AddSeconds(-periodSecondsInterval * i);
                result.Add(ticks.SkipWhile(t => t.Date < startDate).First());
            }

            return result;
        }

        public static IMarketApi GetMarketApi(TradingPair tradingPair)
        {
            using var db = new DatabaseContext();
            TradingPair dbTradingPair = db.TradingPairs.Include(tp => tp.Market).First(tp => tp.TradingPairId == tradingPair.TradingPairId);

            return new MarketApis().First(api => api.Name == dbTradingPair.Market!.Name)!;
        }
    }
}
