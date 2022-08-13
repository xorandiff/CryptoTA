using System;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class containing high, low, open and close prices (for last 
    /// 24h) and currency current volume.
    /// </summary>
    public class TickData
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public DateTime? Date { get; set; }
    }
}
