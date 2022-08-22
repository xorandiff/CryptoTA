using CryptoTA.Database.Models;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class ExponentialMovingAverage : IIndicator
    {
        public string Name => "Exponential Moving Average (EMA)";

        public string Description => "";

        private double Ema(List<Tick> ticks)
        {
            var count = ticks.Count;
            if (count == 0)
            {
                return 0;
            }
            else
            {
                double alpha = 2.0 / (count + 1);
                var lastValue = ticks[count - 1].Close;
                ticks.RemoveAt(count - 1);

                return alpha * lastValue + (1 - alpha) * Ema(ticks);
            }
        }

        public double Run(List<Tick> ticks)
        {
            return Ema(ticks);
        }
    }
}
