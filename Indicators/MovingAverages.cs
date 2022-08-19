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
                new ExponentialMovingAverage()
            };
        }

        public List<IndicatorResult> Run(List<Tick> ticks)
        {
            List<IndicatorResult> result = new();

            foreach (var movingAverage in movingAverages)
            {
                var indicatorValue = movingAverage.Run(ticks);
                var indicatorResult = new IndicatorResult()
                {
                    Name = movingAverage.Name,
                    Value = indicatorValue
                };

                result.Add(indicatorResult);
            }

            return result;
        }
    }
}
