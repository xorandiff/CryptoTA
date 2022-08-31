using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string MarketOrderId { get; set; }

        public string? MarketReferralOrderId { get; set; }

        public string? UserReferenceId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string OrderType { get; set; }

        [Required]
        public string Status { get; set; }

        public string? Description { get; set; }

        [Required]
        public double TotalCost { get; set; }

        public double? AveragePrice { get; set; }

        public double? StopPrice { get; set; }

        public double? LimitPrice { get; set; }

        public double? SecondaryPrice { get; set; }

        public double? Leverage { get; set; }

        [Required]
        public double Fee { get; set; }

        [Required]
        public double Volume { get; set; }

        public double? VolumeExecuted { get; set; }

        [Required]
        public DateTime OpenDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string? Miscellaneous { get; set; }

        public string? Flags { get; set; }

        [ForeignKey("TradingPair"), Required]
        public int TradingPairId { get; set; }

        public TradingPair? TradingPair { get; set; }
    }
}
