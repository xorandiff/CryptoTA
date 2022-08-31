using CryptoTA.Database;
using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class Oscillators
    {
        private readonly List<IIndicator> oscillators;
        public Oscillators()
        {
            oscillators = new()
            {
                new RelativeStrengthIndex(),
                new StochasticOscillator(),
                new CommodityChannelIndex()
            };
        }

        public List<IndicatorResult> Run(List<Tick> ticks, uint secondsInterval, Tick currentTick)
        {
            List<IndicatorResult> result = new();

            if (ticks.Any())
            {
                var measurements = new int[] { 14, 14, 20 };

                var tickPeriods = DatabaseModel.GetTickPeriods(ticks, secondsInterval, measurements.Max());

                for (var i = 0; i < oscillators.Count; i++)
                {
                    result.Add(oscillators[i].Run(tickPeriods.TakeLast(measurements[i]).ToList(), currentTick));
                }
            }

            return result;
        }
    }
}
