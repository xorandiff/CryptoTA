using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    public class MarketApis : ObservableCollection<IMarketApi>
    {
        public MarketApis() : base()
        {
            Add(new BitstampApi());
        }

        public bool IsAnyEnabled
        {
            get
            {
                foreach (var api in this)
            {
                if (api.Enabled)
                {
                    return true;
                }
            }

            return false;
            }
        }
    }
}
