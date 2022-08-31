using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing basic order data.
    /// </summary>
    /// <see cref="TradingPair"/>
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        /// <value>
        /// Order ID used by market.
        /// </value>
        public string MarketOrderId { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public double Cost { get; set; }
        public double Price { get; set; }
        public double Fee { get; set; }
        public double Volume { get; set; }
        public double VolumeExecuted { get; set; }

        /// <value>
        /// Optional client ID, which is included if API is used by more than 
        /// one user.
        /// </value>
        public int? ClientId { get; set; }

        /// <value>
        /// Creation date of current order.
        /// </value>
        public DateTime OpenDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }

        /// <value>
        /// Initial volume (amount) of current order.
        /// </value>
        public double InitialVolume { get; set; }

        /// <value>
        /// Remaining volume (amount) of current order.
        /// </value>
        public double RemainingVolume { get; set; }

        [ForeignKey("Trade")]
        public int TradeId { get; set; }

        [ForeignKey("TradingPair")]
        public int TradingPairId { get; set; }

        /// <value>
        /// Trading pair data for current order.
        /// </value>
        public TradingPair? TradingPair { get; set; }
        public Trade? Trade { get; set; }
    }
}
