
namespace CryptoTA.Apis
{
    /// <summary>
    /// Class which contains maker and taker percentage fees 
    /// and their respective min and max values.
    /// </summary>
    public class Fees
    {
        public double MakerPercent { get; set; }

        public double MakerMin { get; set; }

        public double MakerMax { get; set; }


        public double TakerPercent { get; set; }

        public double TakerMin { get; set; }

        public double TakerMax { get; set; }
    }
}
