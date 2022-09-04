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
        private const int loopInterval = 60 * 1;

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

                        var accountBalance = marketApi.GetAccountBalance(tradingPair);

                        var baseBalance = accountBalance.Where(b => b.Name is not null && tradingPair.BaseSymbol.Contains(b.Name)).FirstOrDefault();
                        var counterBalance = accountBalance.Where(b => b.Name is not null && tradingPair.CounterSymbol.Contains(b.Name)).FirstOrDefault();

                        if (baseBalance?.TotalAmount is not double availableBaseVolume || counterBalance?.TotalAmount is not double availableCounterVolume)
                        {
                            continue;
                        }

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

                        if (indicatorsRatio is null || marketApi.GetTradingFees(tradingPair) is not List<Fees> fees || db.Strategies.Find(strategy.StrategyId) is not Strategy dbStrategy)
                        {
                            continue;
                        }

                        var fee = fees.First();

                        if (databaseModel.GetTickBlocking(tradingPair) is not Tick currentTick)
                        {
                            continue;
                        }

                        double buyVolume = strategy.BuyAmount == 0 ? availableCounterVolume * (strategy.BuyPercentages / 100d) / currentTick.Close : strategy.BuyAmount;
                        double sellVolume = availableBaseVolume;

                        buyVolume = Math.Round(buyVolume, tradingPair.BaseDecimals, MidpointRounding.ToZero);

                        var buyFee = buyVolume * fee.TakerPercent;
                        var sellFee = sellVolume * fee.MakerPercent;

                        if (buyFee < fee.TakerMin)
                        {
                            buyFee = fee.TakerMin;
                        }

                        if (sellFee < fee.MakerMin)
                        {
                            sellFee = fee.MakerMin;
                        }

                        if (buyVolume <= 0d && sellVolume <= 0d || indicatorsRatio > 0.6d && indicatorsRatio < 0.8d)
                        {
                            continue;
                        }

                        var buyNetVolume = buyVolume - buyFee;
                        var sellNetVolume = sellVolume - sellFee;

                        buyNetVolume = tradingPair.MinimalOrderAmount > buyNetVolume ? tradingPair.MinimalOrderAmount : buyNetVolume;
                        sellNetVolume = tradingPair.MinimalOrderAmount > sellNetVolume ? tradingPair.MinimalOrderAmount : sellNetVolume;
                        continue;
                        if (indicatorsRatio <= 0.6d && (strategy.Order is null || strategy.Order.Type != "buy") && buyVolume > 0)
                        {
                            // Buy

                            if (marketApi.BuyOrder(tradingPair, OrderType.Market, buyVolume, currentTick.Close) is not string tradeId)
                            {
                                continue;
                            }

                            //dbStrategy.Order = new Order
                            //{
                            //    MarketOrderId = tradeId,
                            //    Type = "buy",
                            //    OrderType = "market",
                            //    Status = "open",
                            //    TotalCost = buyVolume,
                            //    Fee = buyFee,
                            //    Volume = buyNetVolume,
                            //    OpenDate = currentDate,
                            //    StartDate = currentDate,
                            //    TradingPairId = tradingPair.TradingPairId
                            //};
                        }
                        else if (indicatorsRatio >= 0.8d && (strategy.Order is null || strategy.Order.Type != "sell") && sellVolume > 0)
                        {
                            // Sell

                            if (marketApi.SellOrder(tradingPair, OrderType.Market, sellNetVolume, currentTick.Close) is not string tradeId)
                            {
                                continue;
                            }

                            //dbStrategy.Order = new Order
                            //{
                            //    MarketOrderId = tradeId,
                            //    Type = "sell",
                            //    OrderType = "market",
                            //    Status = "open",
                            //    TotalCost = sellVolume,
                            //    Fee = sellFee,
                            //    Volume = sellNetVolume,
                            //    OpenDate = currentDate,
                            //    StartDate = currentDate,
                            //    TradingPairId = tradingPair.TradingPairId
                            //};
                        }

                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
