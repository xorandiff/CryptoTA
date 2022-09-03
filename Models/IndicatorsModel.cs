using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Indicators;
using System;
using System.Linq;

namespace CryptoTA.Models
{
    public class IndicatorsModel
    {
        private readonly DatabaseModel databaseModel;
        private readonly IndicatorsCollection indicators;

        public IndicatorsModel()
        {
            databaseModel = new();
            indicators = new();
        }

        public double? GetTotalRatio(TradingPair tradingPair, DateTime? startDate, uint secondsIndicatorInterval)
        {
            var marketApi = DatabaseModel.GetMarketApi(tradingPair);

            var ticks = databaseModel.GetTicksBlocking(tradingPair.TradingPairId, startDate, marketApi.RequestMaxTickCount);
            var currentTick = databaseModel.GetTickBlocking(tradingPair)!;

            var indicatorResults = indicators.Run(ticks, secondsIndicatorInterval, currentTick);

            if (!indicatorResults.Any())
            {
                return null;
            }

            var buyCount = indicatorResults.Where(ir => ir.ShouldBuy == true).Count();
            var sellCount = indicatorResults.Where(ir => ir.ShouldBuy == false).Count();
            var neutralCount = indicatorResults.Where(ir => ir.ShouldBuy == null).Count();

            return (buyCount + neutralCount * 0.5d) / (buyCount + neutralCount + sellCount);
        }
    }
}
