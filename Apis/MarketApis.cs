using System.Collections.ObjectModel;

namespace CryptoTA.Apis
{
    public class MarketApis : ObservableCollection<IMarketApi>
    {
        private IMarketApi activeMarketApi;
        public IMarketApi ActiveMarketApi { get { return activeMarketApi; } }

        public MarketApis() : base()
        {
            Add(new BitstampApi());
            activeMarketApi = this[0];
        }

        public bool setActiveApiByName(string marketName)
        {
            foreach (IMarketApi marketApi in this)
            {
                if (marketApi.Name == marketName)
                {
                    activeMarketApi = marketApi;
                    return true;
                }
            }

            return false;
        }
    }
}
