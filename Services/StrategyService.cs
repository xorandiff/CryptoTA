using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Indicators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace CryptoTA.Services
{
    public class StrategyService
    {
        private readonly Dictionary<IMarketApi, Market> apis;
        private readonly MovingAverages movingAverages;
        private readonly Oscillators oscillators;
        private readonly DatabaseModel databaseModel;

        public BackgroundWorker worker;

        public StrategyService()
        {
            worker = new();
            worker.DoWork += Worker_DoWork;
            worker.WorkerSupportsCancellation = true;

            apis = new();
            movingAverages = new();
            oscillators = new();
            databaseModel = new();

            var marketApis = new MarketApis();
            foreach (var marketApi in marketApis)
            {
                using (var db = new DatabaseContext())
                {
                    if (db.Markets.Where(market => market.Name == marketApi.Name).FirstOrDefault() is not Market market)
                    {
                        continue;
                    }

                    apis.Add(marketApi, market);
                }
            }
        }

        public void Run()
        {
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
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

                    using var db = new DatabaseContext();
                    var activeStrategies = db.Strategies
                                            .Where(s => s.Active)
                                            .Include(s => s.TradingPair)
                                            .Include(s => s.Order)
                                            .Where(s => s.TradingPair is TradingPair && s.TradingPair.MarketId == market.MarketId)
                                            .ToArray();

                    foreach (var strategy in activeStrategies)
                    {
                        if (strategy.TradingPair is not TradingPair tradingPair)
                        {
                            continue;
                        }

                        var accountBalance = marketApi.GetAccountBalance();

                        var balance = accountBalance.Where(b => b.Name is not null && tradingPair.Name.Contains(b.Name)).FirstOrDefault();
                        if (balance?.TotalAmount is not double availableBalance || availableBalance == 0d)
                        {
                            continue;
                        }

                        int intervalMultiplier = 1;
                        uint secondsIndicatorInterval = 60 * 15;
                        if (strategy.StrategyCategoryId == 2)
                        {
                            intervalMultiplier = 2;
                            secondsIndicatorInterval = 60 * 60 * 3;
                        }
                        else if (strategy.StrategyCategoryId == 3)
                        {
                            intervalMultiplier = 4;
                            secondsIndicatorInterval = 60 * 60 * 24 * 3;
                        }
                        else if (strategy.StrategyCategoryId == 4)
                        {
                            intervalMultiplier = 8;
                            secondsIndicatorInterval = 60 * 60 * 24 * 7;
                        }

                        var startDate = DateTime.Now.AddSeconds(-marketApi.OhlcMaxDensityTimeInterval * intervalMultiplier);

                        if (databaseModel.GetTickBlocking(tradingPair) is not Tick currentTick)
                        {
                            continue;
                        }

                        var ticks = databaseModel.GetTicksBlocking(tradingPair.TradingPairId, startDate, marketApi.RequestMaxTickCount);

                        var indicatorResults = new List<IndicatorResult>();
                        indicatorResults.AddRange(movingAverages.Run(ticks, secondsIndicatorInterval, currentTick));
                        indicatorResults.AddRange(oscillators.Run(ticks, secondsIndicatorInterval, currentTick));

                        if (indicatorResults is null || indicatorResults.Count == 0)
                        {
                            continue;
                        }

                        var buyCount = indicatorResults.Where(ir => ir.ShouldBuy == true).Count();
                        var sellCount = indicatorResults.Where(ir => ir.ShouldBuy == false).Count();
                        var neutralCount = indicatorResults.Where(ir => ir.ShouldBuy == null).Count();
                        double indicatorsRatio = (buyCount + neutralCount * 0.5d) / (buyCount + neutralCount + sellCount);

                        if (indicatorsRatio > 0.3d && indicatorsRatio < 0.7d)
                        {
                            continue;
                        }

                        if (marketApi.GetTradingFees(tradingPair) is not List<Fees> fees)
                        {
                            continue;
                        }

                        if (fees.First().MakerFee > strategy.MaximalLoss)
                        {
                            continue;
                        }

                        if (db.Strategies.Find(strategy.StrategyId) is not Strategy dbStrategy)
                        {
                            continue;
                        }

                        double orderFee = fees.First().TakerFee;

                        if (indicatorsRatio >= 0.7d && (strategy.Order is null || strategy.Order.Type != "buy"))
                        {
                            // Buy

                            double volume = availableBalance * (strategy.BuyPercentages / 100d);

                            if (marketApi.BuyOrder(tradingPair, OrderType.Instant, volume) is not string tradeId)
                            {
                                continue;
                            }

                            dbStrategy.Order = new Order
                            {
                                MarketOrderId = tradeId,
                                Type = "buy",
                                OrderType = OrderType.Instant.ToString(),
                                Status = "open",
                                TotalCost = volume + orderFee,
                                Fee = orderFee,
                                Volume = volume,
                                OpenDate = DateTime.Now,
                                StartDate = DateTime.Now,
                                TradingPairId = tradingPair.TradingPairId
                            };
                        }
                        else if (indicatorsRatio <= 0.3d && (strategy.Order is null || strategy.Order.Type != "sell"))
                        {
                            // Sell

                            if (marketApi.SellOrder(tradingPair, OrderType.Instant, availableBalance) is not string tradeId)
                            {
                                continue;   
                            }

                            dbStrategy.Order = new Order
                            {
                                MarketOrderId = tradeId,
                                Type = "sell",
                                OrderType = OrderType.Instant.ToString(),
                                Status = "open",
                                TotalCost = availableBalance + orderFee,
                                Fee = orderFee,
                                Volume = availableBalance,
                                OpenDate = DateTime.Now,
                                StartDate = DateTime.Now,
                                TradingPairId = tradingPair.TradingPairId
                            };
                        }

                        db.SaveChanges();
                    }
                }

                Thread.Sleep(1000 * 60 * 15);
            }
        }
    }
}
