using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Indicators
{
    public class MovingAverages
    {
        private readonly List<IIndicator> movingAverages;
        public MovingAverages()
        {
            movingAverages = new()
            {
                new SimpleMovingAverage(),
                //new ExponentialMovingAverage()
            };
        }

        public List<IndicatorResult> Run(List<Tick> ticks, uint secondsInterval)
        {
            List<IndicatorResult> result = new();

            if (ticks.Any())
            {
                var currentPrice = ticks.First().Close;
                var measurements = new List<uint>() { 10, 20, 30, 50, 100, 200 };

                foreach (var measurement in measurements)
                {
                    var pDate = DateTime.Now.AddSeconds(-measurement * secondsInterval);
                    var pTicks = ticks.SkipWhile(t => t.Date < pDate).ToList();

                    foreach (var movingAverage in movingAverages)
                    {
                        var indicatorValue = movingAverage.Run(pTicks);
                        var indicatorResult = new IndicatorResult()
                        {
                            Name = $"{movingAverage.Name} ({measurement})",
                            Value = indicatorValue,
                            ShouldBuy = indicatorValue < currentPrice
                        };

                        result.Add(indicatorResult);
                    }
                }
            }

            return result;
        }
    }
}
