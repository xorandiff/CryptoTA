using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Models;
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
        // 15 minutes loop interval
        private const int loopInterval = 60 * 15;

        private readonly Dictionary<IMarketApi, Market> apis;
        private readonly IndicatorsModel indicatorsModel;

        public BackgroundWorker worker;

        public StrategyService()
        {
            worker = new();
            worker.DoWork += Worker_DoWork;
            worker.WorkerSupportsCancellation = true;

            apis = new();
            indicatorsModel = new();

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

                    var marketTradingPairIds = db.TradingPairs.Where(tp => tp.MarketId == market.MarketId).Select(tp => tp.TradingPairId).ToList();
                    var activeStrategies = db.Strategies.Include(s => s.Order).Include(s => s.TradingPair).Where(s => s.Active && marketTradingPairIds.Contains(s.TradingPairId)).ToArray();

                    foreach (var strategy in activeStrategies)
                    {
                        if (strategy.TradingPair is not TradingPair tradingPair)
                        {
                            continue;
                        }

                        var accountBalance = marketApi.GetAccountBalance(tradingPair);

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

                        var currentDate = DateTime.Now;
                        var startDate = currentDate.AddSeconds(-marketApi.OhlcMaxDensityTimeInterval * intervalMultiplier);

                        var indicatorsRatio = indicatorsModel.GetTotalRatio(tradingPair, startDate, secondsIndicatorInterval);
                        double volume = strategy.BuyAmount == 0 ? availableBalance * (strategy.BuyPercentages / 100d) : strategy.BuyAmount;

                        if (indicatorsRatio is null 
                            || indicatorsRatio > 0.3d && indicatorsRatio < 0.7d
                            || marketApi.GetTradingFees(tradingPair) is not List<Fees> fees
                            || fees.First().MakerFee > strategy.MaximalLoss
                            || db.Strategies.Find(strategy.StrategyId) is not Strategy dbStrategy
                            || volume > availableBalance)
                        {
                            continue;
                        }


                        double orderFee = fees.First().TakerFee;

                        if (indicatorsRatio >= 0.7d && (strategy.Order is null || strategy.Order.Type != "buy"))
                        {
                            // Buy

                            if (marketApi.BuyOrder(tradingPair, OrderType.Instant, volume) is not string tradeId)
                            {
                                continue;
                            }

                            dbStrategy.Order = new Order
                            {
                                MarketOrderId = tradeId,
                                Type = "buy",
                                OrderType = "market",
                                Status = "open",
                                TotalCost = volume + orderFee,
                                Fee = orderFee,
                                Volume = volume,
                                OpenDate = currentDate,
                                StartDate = currentDate,
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
                                OrderType = "market",
                                Status = "open",
                                TotalCost = availableBalance + orderFee,
                                Fee = orderFee,
                                Volume = availableBalance,
                                OpenDate = currentDate,
                                StartDate = currentDate,
                                TradingPairId = tradingPair.TradingPairId
                            };
                        }

                        db.SaveChanges();
                    }
                }

                Thread.Sleep(1000 * loopInterval);
            }
        }
    }
}
