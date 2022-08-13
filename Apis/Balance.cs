
namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing currency name and total balance amount.
    /// </summary>
    public class Balance
    {
        /// <value>
        /// Currency symbol.
        /// <example>USD</example>
        /// </value>
        public string? Name { get; set; }

        /// <value>
        /// Total account/trading amount.
        /// </value>
        public double TotalAmount { get; set; }
    }
}
