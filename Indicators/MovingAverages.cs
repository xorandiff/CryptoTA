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
                new SimpleMovingAverage()
            };
        }

        public List<IndicatorResult> Run(List<Tick> ticks)
        {
            List<IndicatorResult> result = new();

            foreach (var movingAverage in movingAverages)
            {
                var indicatorResult = new IndicatorResult()
                {
                    Name = movingAverage.Name,
                    Value = movingAverage.Run(ticks)
                };

                result.Add(indicatorResult);
            }

            return result;
        }
    }
}
