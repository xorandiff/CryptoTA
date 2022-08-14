using System;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing basic order data.
    /// </summary>
    /// <see cref="TradingPair"/>
    public class Order
    {
        /// <value>
        /// Order ID used by market.
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// Optional client ID, which is included if API is used by more than 
        /// one user.
        /// </value>
        public int? ClientId { get; set; }

        /// <value>
        /// Creation date of current order.
        /// </value>
        public DateTime Date { get; set; }

        /// <value>
        /// Initial volume (amount) of current order.
        /// </value>
        public double InitialVolume { get; set; }

        /// <value>
        /// Remaining volume (amount) of current order.
        /// </value>
        public double RemainingVolume { get; set; }

        /// <value>
        /// Trading pair data for current order.
        /// </value>
        public TradingPair? TradingPair { get; set; }
    }
}
