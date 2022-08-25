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

        public double Run(List<Tick> ticks)
        {
            int periodCount = ticks.Count;
            int halfPeriodCount = periodCount / 2;
            int sqrtPeriodCount = (int) Math.Sqrt(periodCount);

            var wma = new WeightedMovingAverage().Run(ticks);
            var halfPeriodWma = new WeightedMovingAverage().Run(ticks.Take(halfPeriodCount).ToList());
            var sqrtPeriodWma = new WeightedMovingAverage().Run(ticks.Take(sqrtPeriodCount).ToList());

            var result = 2 * halfPeriodWma - wma;

            return result;
        }
    }
}
