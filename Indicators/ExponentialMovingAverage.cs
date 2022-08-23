using System.Linq;
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
            var count = ticks.LongCount();
            if (count == 0)
            {
                return 0;
            }
            else
            {
                double alpha = 2.0 / (count + 1);
                var lastValue = ticks.Last().Close;
                ticks.Remove(ticks.Last());
                return alpha * lastValue + (1 - alpha) * Ema(ticks);
            }
        }

        public double Run(List<Tick> ticks)
        {
            return Ema(ticks);
        }
    }
}
