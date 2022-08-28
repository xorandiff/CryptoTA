using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace CryptoTA.Apis
{
    public class MarketApis : ObservableCollection<IMarketApi>
    {
        private IMarketApi activeMarketApi;
        public IMarketApi ActiveMarketApi { get { return activeMarketApi; } }

        public MarketApis() : base()
        {
            Add(new BitstampApi());
            Add(new KrakenApi());
            activeMarketApi = this[0];
        }

        public static string GenerateNonce()
        {
            var random = RandomNumberGenerator.Create();
            var bytes = new byte[sizeof(uint)];
            random.GetNonZeroBytes(bytes);
            return BitConverter.ToUInt32(bytes).ToString();
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
