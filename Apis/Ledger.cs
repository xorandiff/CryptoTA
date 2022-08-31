
using System;

namespace CryptoTA.Apis
{
    public class Ledger
    {
        public string MarketLedgerId{ get; set; }
        public string ReferenceId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string AssetClass { get; set; }
        public string Asset { get; set; }
        public double Amount { get; set; }
        public double Fee { get; set; }
        public double Balance { get; set; }
    }
}
