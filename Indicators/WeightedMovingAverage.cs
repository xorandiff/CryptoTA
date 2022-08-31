using CryptoTA.Database.Models;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class WeightedMovingAverage : IIndicator
    {
        public string Name => "Weighted Moving Average (WMA)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < ticks.Count; i++)
            {
                nominator += ticks[i].Close * (i + 1);
                denominator += i + 1;
            }

            var wma = nominator / denominator;

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = wma,
                ShouldBuy = currentTick.Close != wma ? (wma < currentTick.Close) : null
            };
        }
    }
}
