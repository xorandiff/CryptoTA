
namespace CryptoTA.Apis
{
    /// <summary>
    /// Class which contains maker and taker fees. Data for both 
    /// type of fees is included, though in application, only taker fee will be used.
    /// </summary>
    public class Fees
    {
        /// <value>
        /// Not used in application for the moment.
        /// </value>
        public double MakerFee { get; set; }

        /// <value>
        /// Used as only fee in application.
        /// </value>
        public double TakerFee { get; set; }
    }
}
