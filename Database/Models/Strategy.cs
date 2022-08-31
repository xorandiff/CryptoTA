
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Strategies")]
    public class Strategy
    {
        [Key]
        public int StrategyId { get; set; }

        [Required]
        public double MinimalGain { get; set; }

        [Required]
        public double MaximalLoss { get; set; }

        [Required]
        public double BuyAmount { get; set; }

        [Required]
        public double BuyPercentages { get; set; }

        [Required]
        public bool AskBeforeTrade { get; set; }

        [Required]
        public bool Active { get; set; }

        [ForeignKey("StrategyCategory"), Required]
        public int StrategyCategoryId { get; set; }

        [ForeignKey("TradingPair"), Required]
        public int TradingPairId { get; set; }

        [ForeignKey("Orders")]
        public int? OrderId { get; set; }

        public virtual StrategyCategory? StrategyCategory { get; set; }

        public virtual TradingPair? TradingPair { get; set; }

        public virtual Order? Order { get; set; }
    }
}
