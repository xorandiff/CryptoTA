using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Ticks")]
    public class Tick
    {
        [Key]
        public int TickId { get; set; }

        [Required]
        public double High { get; set; }

        [Required]
        public double Low { get; set; }

        [Required]
        public double Open { get; set; }

        [Required]
        public double Close { get; set; }

        [Required]
        public double Volume { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [ForeignKey("TradingPair"), Required]
        public int TradingPairId { get; set; }

        public virtual TradingPair TradingPair { get; set; }
    }
}
