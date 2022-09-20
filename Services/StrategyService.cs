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

namespace CryptoTA.Services;

public class StrategyService
{
    // 15 minutes loop interval
    private const int loopInterval = 10;

    private const double indicatorLower = 0.1;
    private const double indicatorUpper = 0.2;

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

                    var currentDate = DateTime.Now;
                    var startDate = currentDate.AddMinutes(-200 * 5);

                    if (strategy.StrategyCategoryId == 2)
                    {
                        startDate = currentDate.AddMinutes(-200 * 30);
                    }
                    else if (strategy.StrategyCategoryId == 3)
                    {
                        startDate = currentDate.AddHours(-6);
                    }
                    else if (strategy.StrategyCategoryId == 4)
                    {
                        startDate = currentDate.AddDays(-4);
                    }

                    var indicatorsInterval = (currentDate - startDate).TotalSeconds;

                    var indicatorsRatio = indicatorsModel.GetTotalRatio(tradingPair, startDate, (uint)indicatorsInterval);

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

                    var feePercent = fee.TakerPercent;

                    var buyCounterVolume = strategy.BuyAmount == 0 ? availableCounterVolume * (strategy.BuyPercentages / 100d) : strategy.BuyAmount;
                    var buyVolume = Math.Round(buyCounterVolume / currentTick.Close, tradingPair.BaseDecimals);
                    buyCounterVolume = Math.Round(buyCounterVolume, tradingPair.CounterDecimals);

                    var sellVolume = availableBaseVolume;
                    var sellCounterVolume = Math.Round(sellVolume * currentTick.Close, tradingPair.CounterDecimals);

                    if (buyVolume == 0d && sellVolume == 0d || indicatorsRatio > indicatorLower && indicatorsRatio < indicatorUpper)
                    {
                        continue;
                    }

                    var buyNetVolume = Math.Round(buyCounterVolume * (1 - feePercent) / currentTick.Close, tradingPair.BaseDecimals);
                    var sellNetVolume = Math.Round(sellCounterVolume * (1 - feePercent) / currentTick.Close, tradingPair.BaseDecimals);

                    var buyMinAmount = tradingPair.MinimalOrderAmount * currentTick.Close;
                    var sellMinAmount = tradingPair.MinimalOrderAmount;

                    if (indicatorsRatio >= indicatorLower && (strategy.Order is null || strategy.Order.Type != "buy") && buyNetVolume >= tradingPair.MinimalOrderAmount)
                    {
                        // Buy

                        if (marketApi.BuyOrder(tradingPair, OrderType.Market, buyNetVolume, currentTick.Close) is not string tradeId)
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
                    else if (indicatorsRatio <= indicatorUpper && (strategy.Order is null || strategy.Order.Type != "sell") && sellNetVolume >= tradingPair.MinimalOrderAmount)
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
