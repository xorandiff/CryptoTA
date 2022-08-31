using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace CryptoTA.Indicators
{
    public class StochasticOscillator : IIndicator
    {
        public string Name => "Stochastic oscillator (STOCH) - line %K";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            double newestClosingPrice = currentTick.Close;
            double lowestLowPrice = ticks.OrderBy(t => t.Low).First().Low;
            double highestHighPrice = ticks.OrderByDescending(t => t.High).First().High;

            var stoch = (newestClosingPrice - lowestLowPrice) / (highestHighPrice - lowestLowPrice) * 100d;

            bool? shouldBuy = null;
            if (stoch >= 80)
            {
                shouldBuy = false;
            }
            else if (stoch <= 20)
            {
                shouldBuy = true;
            }

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = stoch,
                ShouldBuy = shouldBuy
            };
        }
    }
}
