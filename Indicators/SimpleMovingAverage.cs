using CryptoTA.Database.Models;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class SimpleMovingAverage : IIndicator
    {
        public string Name => "Simple Moving Average (SMA)";

        public string Description => "";

        public double Run(List<Tick> ticks)
        {
            double result = 0;

            foreach (var tick in ticks)
            {
                result += tick.Close;
            }

            return ticks.Count > 0 ? result / ticks.Count : 0;
        }
    }
}
