using CryptoTA.Database.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CryptoTA.Indicators
{
    public class IndicatorsCollection : ObservableCollection<IIndicator>
    {
        private readonly MovingAverages movingAverages = new();
        private readonly Oscillators oscillators = new();

        public IndicatorsCollection() : base()
        {
            var indicators = movingAverages.IndicatorsCollection;
            indicators.AddRange(oscillators.IndicatorsCollection);

            foreach (IIndicator indicator in indicators)
            {
                Add(indicator);
            }
        }

        public List<IndicatorResult> Run(List<Tick> ticks, uint secondsInterval, Tick currentTick)
        {
            var movingAveragesResults = movingAverages.Run(ticks, secondsInterval, currentTick);
            var oscillatorsResults = movingAverages.Run(ticks, secondsInterval, currentTick);

            var results = movingAveragesResults;
            results.AddRange(oscillatorsResults);

            return results;
        }
    }
}
