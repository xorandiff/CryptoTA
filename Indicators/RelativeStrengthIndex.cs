using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    public class RelativeStrengthIndex : IIndicator
    {
        public string Name => "Relative Strength Index (RSI)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            List<Tick> relativeStrengthUpward = new();
            List<Tick> relativeStrengthDownward = new();

            for (int i = 0; i < ticks.Count - 1; i++)
            {
                double change = ticks[i + 1].Close - ticks[i].Close;
                if (change > 0)
                {
                    relativeStrengthUpward.Add(new Tick { Close = change });
                    relativeStrengthDownward.Add(new Tick { Close = 0d });
                }
                else if (change < 0)
                {
                    relativeStrengthUpward.Add(new Tick { Close = 0d });
                    relativeStrengthDownward.Add(new Tick { Close = Math.Abs(change) });
                }
                else
                {
                    relativeStrengthUpward.Add(new Tick { Close = 0d });
                    relativeStrengthDownward.Add(new Tick { Close = 0d });
                }
            }

            double upwardAverage = new SmoothedMovingAverage().Run(relativeStrengthUpward, currentTick).Value;
            double downwardAverage = new SmoothedMovingAverage().Run(relativeStrengthDownward, currentTick).Value;

            double rsi;

            if (downwardAverage == 0)
            {
                rsi = 100d;
            }
            else
            {
                double relativeStrength = upwardAverage * 1d / downwardAverage;
                rsi = 100d - (100d / (1d + relativeStrength));
            }

            bool? shouldBuy = null;
            if (rsi >= 70)
            {
                shouldBuy = false;
            }
            else if (rsi <= 30)
            {
                shouldBuy = true;
            }

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = rsi,
                ShouldBuy = shouldBuy
            };
        }
    }
}
