using CryptoTA.Database.Models;
using System.Collections.Generic;

namespace CryptoTA.Indicators
{
    internal interface IIndicator
    {
        public string Name { get; }
        public string Description { get; }
        public IndicatorResult Run(List<Tick> ticks, Tick currentTick);
    }
}
