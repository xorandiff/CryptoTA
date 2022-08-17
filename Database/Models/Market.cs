using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CryptoTA.Database.Models
{
    /// <summary>
    /// Market model class.
    /// </summary>
    public class Market
    {
        /// <value>
        /// Market database ID.
        /// </value>
        [Key]
        public int MarketId { get; set; }

        /// <value>
        /// Market displayed name.
        /// </value>
        [Required]
        public string Name { get; set; }

        /// <value>
        /// If there are API calls that don't need 
        /// credentials, then it's set to <c>true</c>, 
        /// otherwise it's <c>faLse</c>.
        /// </value>
        [Required]
        public bool CredentialsRequired { get; set; }

        /// <value>
        /// Corresponding <c>TradingPair</c> list.
        /// </value>
        public virtual List<TradingPair> TradingPairs { get; set; }

        /// <value>
        /// Corresponding <c>Credentials</c> list.
        /// </value>
        public virtual List<Credentials> Credentials { get; set; }
    }
}
