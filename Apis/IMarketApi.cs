using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    class TradingPair
    {
        public string? Name { get; set; }
        public string? AlternativeName { get; set; }
        public string? WebsocketName { get; set; }
        public string? BaseName { get; set; }
        public string? CounterName { get; set; }
        public string? BaseSymbol { get; set; }
        public string? CounterSymbol { get; set; }

        public uint BaseDecimals { get; set; }
        public uint CounterDecimals { get; set; }
        public double MinimalOrderAmount { get; set; }
    }

    class Fees
    {
        public double MakerFee { get; set; }
        public double TakerFee { get; set; }
    }

    class TickData
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }

    class Balance
    {
        public string? Name { get; set; }
        public double TotalAmount { get; set; }
    }

    class TradingBalance : Balance
    {
        public double AvailableAmount { get; set; }
        public double ReservedAmount { get; set; }
    }

    class OrderBookEntry
    {
        public double Price { get; set; }
        public double Volume { get; set; }
        public DateTime Date { get; set; }
    }

    class OrderBook
    {
        public OrderBookEntry[]? Asks { get; set; }
        public OrderBookEntry[]? Bids { get; set; }
    }

    enum OrderType
    {
        Market,
        Limit,
        Instant
    }

    class Order
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public DateTime Date { get; set; }
        public double InitialVolume { get; set; }
        public double RemainingVolume { get; set; }
        public TradingPair? TradingPair { get; set; }
    }

    class WebsocketsToken
    {
        public string? Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    internal interface IMarketApi
    {
        public Task<TradingPair[]> GetTradingPairs();
        public Task<uint[]> GetOhlcTimeIntervals(TradingPair tradingPair);
        public Task<TickData[]> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval);
        public Task<TickData> GetTick(TradingPair tradingPair);
        public Task<Fees[]> GetTradingFees(TradingPair tradingPair);
        public Task<Fees[]> GetWithdrawalFees(TradingPair tradingPair);
        public Task<Balance[]> GetAccountBalance();
        public Task<Balance[]> GetTradingBalance();
        public Task<OrderBook> GetOrderBook();
        public Task<int> BuyOrder(OrderType orderType, double amount, double price = 0);
        public Task<int> SellOrder(OrderType orderType, double amount, double price = 0);
        public Task<bool> CancelOrder(int orderId);
        public Task<bool> CancelAllOrders();
        public Task<Order[]> GetOpenOrders();
        public Task<Order[]> GetClosedOrders();
        public Task<WebsocketsToken> GetWebsocketsToken();
    }
}
