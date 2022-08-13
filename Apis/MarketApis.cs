using System.Collections.ObjectModel;

namespace CryptoTA.Apis
{
    public class MarketApis : ObservableCollection<IMarketApi>
    {
        public MarketApis() : base()
        {
            Add(new BitstampApi());
        }
    }
}
