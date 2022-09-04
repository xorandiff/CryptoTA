using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Database.Models
{
    [Table("TradingPairs")]
    public class TradingPair
    {
        /// <value>
        /// Database ID.
        /// </value>
        [Key]
        public int TradingPairId { get; set; }

        /// <value>
        /// Name of trading pair used for identification in API.
        /// <example>XETHXXBT</example>
        /// <example>ETH/XBT</example>
        /// <example>ethxbt</example>
        /// </value>
        [Required]
        public string Name { get; set; }
        /// <value>
        /// Optional alternative name delivered by API. Used in case 
        /// <paramref name="Name"/> fails.
        /// </value>
        public string? AlternativeName { get; set; }
        /// <value>
        /// Optional websockets name delivered by API. If not present, 
        /// then <paramref name="Name"/> will be used.
        /// </value>
        public string? WebsocketName { get; set; }

        /// <value>
        /// Base currency full name.
        /// <example>For trading pair ETH/USD, base currency name would 
        /// be Etherum.</example>
        /// </value>
        [Required]
        public string BaseName { get; set; }

        /// <value>
        /// Counter currency full name.
        /// <example>For trading pair ETH/USD, counter currency name would 
        /// be US Dollar.</example>
        /// </value>
        [Required]
        public string CounterName { get; set; }

        /// <value>
        /// Base currency symbol.
        /// <example>For trading pair ETH/USD, base currency symbol would 
        /// be ETH.</example>
        /// </value>
        [Required]
        public string BaseSymbol { get; set; }

        /// <value>
        /// Counter currency symbol.
        /// <example>For trading pair ETH/USD, counter currency symbol would 
        /// be USD.</example>
        /// </value>
        [Required]
        public string CounterSymbol { get; set; }

        /// <value>
        /// Amount of decimal places of the base volume. Shouldn't be  
        /// more than 18 (database precision limit). 
        /// 
        /// WARNING: You should make sure this value matches the value  
        /// given by the market. If it does not match, then for example, 
        /// application may think that market's user has positive volume
        /// of some asset, but if market precision is lower, then in  
        /// fact, market rounds the volume to 0, so market sees no volume.
        /// </value>
        [Required]
        public int BaseDecimals { get; set; }

        /// <value>
        /// Amount of decimal places of the counter volume. Shouldn't be  
        /// more than 18 (database precision limit). 
        /// 
        /// WARNING: You should make sure this value matches the value  
        /// given by the market. If it does not match, then for example, 
        /// application may think that market's user has positive volume
        /// of some asset, but if market precision is lower, then in  
        /// fact, market rounds the volume to 0, so market sees no volume.
        /// </value>
        [Required]
        public int CounterDecimals { get; set; }

        /// <summary>
        /// Minimal amount of base volum required by market for order operations.
        /// </summary>
        [Required]
        public double MinimalOrderAmount { get; set; }

        /// <value>
        /// Foreign key for Market model.
        /// </value>
        [ForeignKey("Market")]
        public int MarketId { get; set; }

        /// <value>
        /// Corresponding Market model.
        /// </value>
        public virtual Market? Market { get; set; }


        /// <value>
        /// List of corresponding Ticks.
        /// </value>
        public virtual List<Tick> Ticks { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (BaseSymbol is not null && CounterSymbol is not null)
                {
                    return $"{BaseSymbol.ToUpper()}/{CounterSymbol.ToUpper()}";
                }
                else
                {
                    return "-";
                }
            }
        }
    }
}
