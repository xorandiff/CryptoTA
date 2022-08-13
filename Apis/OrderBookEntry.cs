using System;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing ask/bid order data.
    /// </summary>
    public class OrderBookEntry
    {
        /// <value>
        /// Order's price in base currency.
        /// </value>
        public double Price { get; set; }

        /// <value>
        /// Order's volume (amount) in base currency.
        /// </value>
        public double Volume { get; set; }

        /// <value>
        /// Order's creation date.
        /// </value>
        public DateTime Date { get; set; }
    }
}
