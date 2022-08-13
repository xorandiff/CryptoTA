
namespace CryptoTA.Models
{
    public class Credentials
    {
        public int CredentialsId { get; set; }
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; }
        public int MarketId { get; set; }
        public virtual Market? Market { get; set; }
    }
}
