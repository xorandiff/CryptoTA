
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("StrategyCategory")]
    public class StrategyCategory
    {
        [Key]
        public int StrategyCategoryId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
