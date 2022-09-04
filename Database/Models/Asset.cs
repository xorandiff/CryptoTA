using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("Assets")]
    public class Asset
    {
        /// <value>
        /// Database ID.
        /// </value>
        [Key]
        public int AssetId { get; set; }

        /// <value>
        /// Name of the asset used for identification in API.
        /// <example>XETH</example>
        /// <example>ETH</example>
        /// <example>eth</example>
        /// </value>
        [Required]
        public string MarketName { get; set; }

        /// <value>
        /// Asset's alternative symbol. If market doesn't deliver one, it should 
        /// match the value of <paramref name="MarketName"/>. It is used as diplay 
        /// symbol in the application, so it's required.
        /// <example>ETH</example>
        /// <example>eth</example>
        /// </value>
        [Required]
        public string AlternativeSymbol { get; set; }

        /// <value>
        /// Asset's full name.
        /// <example>For asset with <paramref name="MarketName"/> "XUSD", asset's 
        /// name would be "US Dollar".</example>
        /// </value>
        [Required]
        public string Name { get; set; }

        /// <value>
        /// Amount of decimal places of the asset's volume. Shouldn't be  
        /// more than 18 (database precision limit). 
        /// </value>
        [Required]
        public int Decimals { get; set; }

        /// <value>
        /// Amount of the display decimal places of the asset's volume. Shouldn't be  
        /// more than 18 (database precision limit). 
        /// </value>
        [Required]
        public int DisplayDecimals { get; set; }

        /// <value>
        /// Foreign key for Market model.
        /// </value>
        [ForeignKey("Market"), Required]
        public int MarketId { get; set; }

        /// <value>
        /// Corresponding Market model.
        /// </value>
        public virtual Market? Market { get; set; }

        /// <value>
        /// Computed property used for displaying 
        /// <paramref name="MarketName"/> in the application.
        /// </value>
        [NotMapped]
        public string Symbol
        {
            get
            {
                if (AlternativeSymbol is not null)
                {
                    return AlternativeSymbol.ToUpper();
                }
                else
                {
                    return "-";
                }
            }
        }
    }
}
