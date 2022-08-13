
namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing <c>Balance</c> data with available and 
    /// reserved balance amounts.
    /// </summary>
    public class TradingBalance : Balance
    {
        /// <value>
        /// Available balance for creating orders.
        /// </value>
        public double AvailableAmount { get; set; }

        /// <value>
        /// Reserved balance by user's orders.
        /// </value>
        public double ReservedAmount { get; set; }
    }
}
