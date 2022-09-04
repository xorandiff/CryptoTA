
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Settings")]
    public class Settings
    {
        [Key]
        public int SettingsId { get; set; }
        public int TimeIntervalIdChart { get; set; }
        public int TimeIntervalIdIndicators { get; set; }
        public int StrategyId { get; set; }

        [ForeignKey("TradingPair")]
        public int TradingPairId { get; set; }

        public virtual TradingPair TradingPair { get; set; }
    }
}
