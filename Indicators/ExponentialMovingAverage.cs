using System.Linq;
using CryptoTA.Database.Models;
using System.Collections.Generic;
using System;

namespace CryptoTA.Indicators
{
    public class ExponentialMovingAverage : IIndicator
    {
        public string Name => "Exponential Moving Average (EMA)";

        public string Description => "";

        public double Run(List<Tick> ticks)
        {
            var ticksCount = ticks.Count;
            double alpha = 2.0 / (ticksCount + 1);
            double alphaP = 1 - alpha;

            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < ticksCount; i++)
            {
                var pow = Math.Pow(alphaP, ticksCount - 1 - i);
                nominator += ticks[i].Close * pow;
                denominator += pow;
            }

            return nominator / denominator;
        }
    }
}
