using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class SmoothedMovingAverage : IIndicator
    {
        public string Name => "Smoothed Moving Average (SMMA)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            var ticksCount = ticks.Count;

            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < ticksCount; i++)
            {
                var pow = Math.Pow(1 - 1d / (i + 1), i);
                nominator += ticks[i].Close * pow;
                denominator += pow;
            }

            var smma = nominator / denominator;

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = smma,
                ShouldBuy = currentTick.Close != smma ? (smma < currentTick.Close) : null
            };
        }
    }
}
