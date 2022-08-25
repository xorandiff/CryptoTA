using CryptoTA.Database.Models;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class WeightedMovingAverage : IIndicator
    {
        public string Name => "Weighted Moving Average (WMA)";

        public string Description => "";

        public double Run(List<Tick> ticks)
        {
            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < ticks.Count; i++)
            {
                nominator += ticks[i].Close * (i + 1);
                denominator += i + 1;
            }

            return nominator / denominator;
        }
    }
}
