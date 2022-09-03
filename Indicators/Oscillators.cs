using CryptoTA.Database;
using CryptoTA.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class Oscillators
    {
        public List<IIndicator> IndicatorsCollection { get; }

        public Oscillators()
        {
            IndicatorsCollection = new()
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

                for (var i = 0; i < IndicatorsCollection.Count; i++)
                {
                    result.Add(IndicatorsCollection[i].Run(tickPeriods.TakeLast(measurements[i]).ToList(), currentTick));
                }
            }

            return result;
        }
    }
}
