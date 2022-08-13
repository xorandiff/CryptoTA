using System.Collections.Generic;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing asks and bids lists.
    /// </summary>
    /// <see cref="OrderBookEntry"/>
    public class OrderBook
    {
        public List<OrderBookEntry>? Asks { get; set; }
        public List<OrderBookEntry>? Bids { get; set; }
    }
}
