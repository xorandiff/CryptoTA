using CryptoTA.Database.Models;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class CommodityChannelIndex : IIndicator
    {
        public string Name => "Commodity Channel Index (CCI)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            var typicalPrices = ticks.Select(t => new Tick { Close = (t.Close + t.Low + t.High) / 3d }).ToList();

            double typicalPrice = (currentTick.Close + currentTick.Low + currentTick.High) / 3d;

            double smaTypicalPrice = new SimpleMovingAverage().Run(typicalPrices, currentTick).Value;
            double meanAbsoluteDerivation = new MeanAbsoluteDerivation().Run(typicalPrices.Select(t => t.Close).ToList());

            var cci = 1d / 0.015d + ((typicalPrice - smaTypicalPrice) * 1d / meanAbsoluteDerivation);

            bool? shouldBuy = null;
            if (cci >= 100)
            {
                shouldBuy = false;
            }
            else if (cci <= -100)
            {
                shouldBuy = true;
            }

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = cci,
                ShouldBuy = shouldBuy
            };
        }
    }
}
