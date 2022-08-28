using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Trades")]
    public class Trade
    {
        [Key]
        public int TradeId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string MarketTradeId { get; set; }

        [Required]
        public string MarketOrderId { get; set; }

        public double Cost { get; set; }

        public double Price { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double Fee { get; set; }

        public double Volume { get; set; }

        [ForeignKey("TradingPair")]
        public int TradingPairId { get; set; }

        public virtual TradingPair? TradingPair { get; set; }
    }
}
