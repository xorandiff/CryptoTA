using CryptoTA.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class SimpleMovingAverage : IIndicator
    {
        public string Name => "Simple Moving Average (SMA)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            double sma = ticks.Average(t => t.Close);

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = sma,
                ShouldBuy = currentTick.Close != sma ? (sma < currentTick.Close) : null
            };
        }
    }
}
