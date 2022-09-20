using CryptoTA.Exceptions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CryptoTA.Apis
{
    public class MarketApis : ObservableCollection<IMarketApi>
    {
        private IMarketApi activeMarketApi;
        public IMarketApi ActiveMarketApi => activeMarketApi;

        public MarketApis() : base()
        {
            Add(new BitstampApi());
            Add(new KrakenApi());

            activeMarketApi = this[0];
        }

        public void SetActiveApiByName(string marketName)
        {
            if (this.Where(a => a.Name == marketName).FirstOrDefault() is not IMarketApi marketApi)
            {
                throw new ApiException($"Market API with name {marketName} doesn't exist or hasn't been properly configured.");
            }

            activeMarketApi = marketApi;
        }
    }
}
