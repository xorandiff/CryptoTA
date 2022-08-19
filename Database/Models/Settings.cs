
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    public class Settings
    {
        [Key]
        public int SettingsId { get; set; }

        [ForeignKey("TradingPair")]
        public int TradingPairId { get; set; }

        [ForeignKey("TimeInterval")]
        public int TimeIntervalId { get; set; }

        public virtual TradingPair TradingPair { get; set; }
        public virtual TimeInterval TimeInterval { get; set; }
    }
}
