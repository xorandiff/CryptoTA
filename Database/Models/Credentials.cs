
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Credentials")]
    public class Credentials
    {
        [Key]
        public int CredentialsId { get; set; }

        [Required]
        public string PublicKey { get; set; }

        [Required]
        public string PrivateKey { get; set; }

        [ForeignKey("Market"), Required]
        public int MarketId { get; set; }

        public virtual Market Market { get; set; }
    }
}
