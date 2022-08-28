using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Indicators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace CryptoTA.Services
{
    public class StrategyService
    {
        private Dictionary<IMarketApi, Market> apis;
        private MovingAverages movingAverages;
        private DatabaseModel databaseModel;

        public BackgroundWorker worker;

        public StrategyService()
        {
            worker = new();
            worker.DoWork += Worker_DoWork;
            worker.WorkerReportsProgress = true;

            apis = new();
            movingAverages = new();
            databaseModel = new();

            var marketApis = new MarketApis();
            foreach (var marketApi in marketApis)
            {
                using (var db = new DatabaseContext())
                {
                    if (db.Markets.Where(market => market.Name == marketApi.Name).FirstOrDefault() is not Market market)
                    {
                        throw new Exception("Error during initialization of StrategyService");
                    }

                    apis.Add(marketApi, market);
                }
            }
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (sender == null)
            {
                e.Cancel = true;
                return;
            }

            while (true)
            {
                foreach (var api in apis)
                {
                    var marketApi = api.Key;
                    var market = api.Value;

                    using (var db = new DatabaseContext())
                    {
                        var activeStrategies = db.Strategies
                                                        .Where(s => s.Active)
                                                        .Include(s => s.TradingPair)
                                                        .Where(s => s.TradingPair.MarketId == market.MarketId)
                                                        .ToArray();

                        foreach (var strategy in activeStrategies)
                        {
                            var tradingPair = strategy.TradingPair;
                            var accountBalance = marketApi.GetAccountBalance();

                            double availableBalance;

                            var isBaseCurrency = accountBalance.Where(ab => tradingPair.Name.StartsWith(ab.Name!)).Any();
                            if (isBaseCurrency)
                            {
                                availableBalance = accountBalance.Where(ab => tradingPair.Name.StartsWith(ab.Name!)).First().TotalAmount;
                            }
                            else
                            {
                                availableBalance = accountBalance.Where(ab => tradingPair.Name.EndsWith(ab.Name!)).First().TotalAmount;
                            }

                            if (availableBalance >= strategy.BuyAmount)
                            {
                                int intervalMultiplier = 1;
                                uint secondsIndicatorInterval = 60 * 15;
                                if (strategy.BuyIndicatorCategory == 2)
                                {
                                    intervalMultiplier = 2;
                                    secondsIndicatorInterval = 60 * 60 * 3;
                                }
                                else if (strategy.BuyIndicatorCategory == 3)
                                {
                                    intervalMultiplier = 4;
                                    secondsIndicatorInterval = 60 * 60 * 24 * 3;
                                }
                                else if (strategy.BuyIndicatorCategory == 4)
                                {
                                    intervalMultiplier = 8;
                                    secondsIndicatorInterval = 60 * 60 * 24 * 7;
                                }

                                var startDate = DateTime.Now.AddSeconds(-marketApi.OhlcMaxDensityTimeInterval * intervalMultiplier);

                                var ticks = databaseModel.GetTicksBlocking(tradingPair.TradingPairId, startDate, marketApi.RequestMaxTickCount);
                                var movingAveragesResult = movingAverages.Run(ticks, secondsIndicatorInterval);

                                if (movingAveragesResult != null)
                                {
                                    var movingAveragesBuyCount = movingAveragesResult.Where(ir => ir.ShouldBuy).Count();
                                    var movingAveragesSellCount = movingAveragesResult.Where(ir => !ir.ShouldBuy).Count();
                                    double movingAveragesCountRatio = movingAveragesBuyCount / (movingAveragesBuyCount + movingAveragesSellCount);

                                    if (movingAveragesCountRatio >= 0.8)
                                    {
                                        // Buy
                                        //var tradeId = marketApi.BuyOrder(OrderType.Instant, availableBalance);
                                    }
                                    else if (movingAveragesCountRatio <= 0.5)
                                    {
                                        // Try sell
                                        //var fees = marketApi.GetTradingFees(tradingPair);
                                        //var tradeId = marketApi.SellOrder(OrderType.Instant, availableBalance);
                                    }
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(1000 * 60 * 15);
            }
        }
    }
}
