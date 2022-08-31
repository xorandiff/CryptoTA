using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTA.Apis
{
    [Table("Trades")]
    public class Trade
    {
        public int TradeId { get; set; }

        public string Type { get; set; }

        public string MarketTradeId { get; set; }

        public string MarketOrderId { get; set; }

        public string OrderType { get; set; }

        public double Cost { get; set; }

        public double Price { get; set; }

        public DateTime Date { get; set; }

        public double Fee { get; set; }

        public double Volume { get; set; }

        public int TradingPairId { get; set; }
    }
}
