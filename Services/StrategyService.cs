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
        private const int loopInterval = 10;

        private readonly Dictionary<IMarketApi, Market> apis;
        private readonly DatabaseModel databaseModel;
        private readonly IndicatorsModel indicatorsModel;

        public BackgroundWorker worker;

        public StrategyService()
        {
            worker = new();
            worker.DoWork += Worker_DoWork;
            worker.WorkerSupportsCancellation = true;

            apis = new();
            databaseModel = new();
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
                Thread.Sleep(1000 * loopInterval);

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

                        var accountBalance = marketApi.GetAccountBalance();

                        var availableBaseVolume = accountBalance.Where(b => b.Name == tradingPair.BaseSymbol).FirstOrDefault()?.TotalAmount ?? 0;
                        var availableCounterVolume = accountBalance.Where(b => b.Name == tradingPair.CounterSymbol).FirstOrDefault()?.TotalAmount ?? 0;

                        availableBaseVolume = Math.Round(availableBaseVolume, tradingPair.BaseDecimals);
                        availableCounterVolume = Math.Round(availableCounterVolume, tradingPair.CounterDecimals);

                        if (availableBaseVolume == 0d && availableCounterVolume == 0d)
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

                        if (indicatorsRatio is null || db.Strategies.Find(strategy.StrategyId) is not Strategy dbStrategy)
                        {
                            continue;
                        }

                        if (marketApi.GetTradingFees(tradingPair, availableBaseVolume) is not Fees fee)
                        {
                            continue;
                        }

                        if (databaseModel.GetTickBlocking(tradingPair) is not Tick currentTick)
                        {
                            continue;
                        }

                        double feePercent = fee.TakerPercent;

                        double buyCounterVolume = strategy.BuyAmount == 0 ? availableCounterVolume * (strategy.BuyPercentages / 100d) / currentTick.Close : strategy.BuyAmount;
                        double buyVolume = Math.Round(buyCounterVolume / currentTick.Close, tradingPair.BaseDecimals);
                        buyCounterVolume = Math.Round(buyCounterVolume, tradingPair.CounterDecimals);

                        double sellVolume = availableBaseVolume;
                        double sellCounterVolume = Math.Round(sellVolume * currentTick.Close, tradingPair.CounterDecimals);

                        if (buyVolume == 0d && sellVolume == 0d || indicatorsRatio > 0.6d && indicatorsRatio < 0.8d)
                        {
                            continue;
                        }

                        double buyNetVolume = Math.Round(buyCounterVolume * (1 - feePercent) / currentTick.Close, tradingPair.BaseDecimals);
                        double sellNetVolume = Math.Round(sellCounterVolume * (1 - feePercent) / currentTick.Close, tradingPair.BaseDecimals);

                        double buyMinAmount = tradingPair.MinimalOrderAmount * currentTick.Close;
                        double sellMinAmount = tradingPair.MinimalOrderAmount;

                        if (indicatorsRatio <= 0.6d && (strategy.Order is null || strategy.Order.Type != "buy") && buyVolume >= buyMinAmount)
                        {
                            // Buy

                            if (marketApi.BuyOrder(tradingPair, OrderType.Market, buyVolume, currentTick.Close) is not string tradeId)
                            {
                                continue;
                            }

                            if (marketApi.GetOrdersInfo(new string[] { tradeId }).First() is not Order order)
                            {
                                continue;
                            }

                            dbStrategy.Order = order;

                            db.SaveChanges();
                        }
                        else if (indicatorsRatio >= 0.8d && (strategy.Order is null || strategy.Order.Type != "sell") && sellVolume >= sellMinAmount)
                        {
                            // Sell

                            if (marketApi.SellOrder(tradingPair, OrderType.Market, sellNetVolume, currentTick.Close) is not string tradeId)
                            {
                                continue;
                            }

                            if (marketApi.GetOrdersInfo(new string[] { tradeId }).First() is not Order order)
                            {
                                continue;
                            }

                            dbStrategy.Order = order;

                            db.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
