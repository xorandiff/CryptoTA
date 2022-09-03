using CryptoTA.Database;
using CryptoTA.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class MovingAverages
    {
        public List<IIndicator> IndicatorsCollection { get; }

        public MovingAverages()
        {
            IndicatorsCollection = new()
            {
                new SimpleMovingAverage(),
                new ExponentialMovingAverage()
            };
        }

        public List<IndicatorResult> Run(List<Tick> ticks, uint secondsInterval, Tick currentTick)
        {
            List<IndicatorResult> result = new();

            if (ticks.Any())
            {
                var measurements = new List<int>() { 10, 20, 30, 50, 100, 200 };
                var tickPeriods = DatabaseModel.GetTickPeriods(ticks, secondsInterval, 200);

                foreach (var measurement in measurements)
                {
                    foreach (var movingAverage in IndicatorsCollection)
                    {
                        result.Add(movingAverage.Run(tickPeriods.TakeLast(measurement).ToList(), currentTick));
                    }
                }
            }

            return result;
        }
    }
}
