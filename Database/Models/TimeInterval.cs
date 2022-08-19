
using System.ComponentModel.DataAnnotations;

namespace CryptoTA.Database.Models
{
    public class TimeInterval
    {
        [Key]
        public int TimeIntervalId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public uint Seconds { get; set; }
    }
}
