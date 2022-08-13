
namespace CryptoTA.Models
{
    /// <summary>
    /// Trading pair model class. 
    /// Contains basic trading pair data with minimal order amount.
    /// </summary>
    public class TradingPair
    {
        /// <value>
        /// Database ID.
        /// </value>
        public int TradingPairId { get; set; }

        /// <value>
        /// Name of trading pair used for identification in API.
        /// <example>XETHXXBT</example>
        /// <example>ETH/XBT</example>
        /// <example>ethxbt</example>
        /// </value>
        public string? Name { get; set; }
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
        public string? BaseName { get; set; }

        /// <value>
        /// Counter currency full name.
        /// <example>For trading pair ETH/USD, counter currency name would 
        /// be US Dollar.</example>
        /// </value>
        public string? CounterName { get; set; }

        /// <value>
        /// Base currency symbol.
        /// <example>For trading pair ETH/USD, base currency symbol would 
        /// be ETH.</example>
        /// </value>
        public string? BaseSymbol { get; set; }

        /// <value>
        /// Counter currency symbol.
        /// <example>For trading pair ETH/USD, counter currency symbol would 
        /// be USD.</example>
        /// </value>
        public string? CounterSymbol { get; set; }

        /// <value>
        /// Amount of decimal places of base currency. Shouldn't be more 
        /// than 18 (database precision limit).
        /// </value>
        public uint BaseDecimals { get; set; }

        /// <value>
        /// Amount of decimal places of counter currency. Shouldn't be more 
        /// than 18 (database precision limit).
        /// </value>
        public uint CounterDecimals { get; set; }

        /// <summary>
        /// Minimal amount required by market for order operations.
        /// </summary>
        public double MinimalOrderAmount { get; set; }

        /// <value>
        /// Foreign key for market
        /// </value>
        public int MarketId { get; set; }

        /// <value>
        /// Corresponding Market model.
        /// </value>
        public virtual Market? Market { get; set; }
    }
}
