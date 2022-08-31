using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class HullMovingAverage : IIndicator
    {
        public string Name => "Hull Moving Average (HMA)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            int periodCount = ticks.Count;
            int halfPeriodCount = periodCount / 2;
            int sqrtPeriodCount = (int) Math.Sqrt(periodCount);

            var wma = new WeightedMovingAverage().Run(ticks, currentTick).Value;
            var halfPeriodWma = new WeightedMovingAverage().Run(ticks.Take(halfPeriodCount).ToList(), currentTick).Value;
            var sqrtPeriodWma = new WeightedMovingAverage().Run(ticks.Take(sqrtPeriodCount).ToList(), currentTick).Value;

            var hma = 2 * halfPeriodWma - wma;

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = hma,
                ShouldBuy = currentTick.Close != hma ? (hma < currentTick.Close) : null
            };
        }
    }
}
