using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class which contains basic trading pair data 
    /// with minimal order amount.
    /// </summary>
    class TradingPair
    {
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
    }

    /// <summary>
    /// Class which contains maker and taker fees. Data for both 
    /// type of fees is included, though in application, only taker fee will be used.
    /// </summary>
    class Fees
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

    /// <summary>
    /// Class containing high, low, open and close prices (for last 
    /// 24h) and currency current volume.
    /// </summary>
    class TickData
    {
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public DateTime? Date { get; set; }
    }

    /// <summary>
    /// Class containing currency name and total balance amount.
    /// </summary>
    class Balance
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

    /// <summary>
    /// Class containing <c>Balance</c> data with available and 
    /// reserved balance amounts.
    /// </summary>
    class TradingBalance : Balance
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

    /// <summary>
    /// Class containing ask/bid order data.
    /// </summary>
    class OrderBookEntry
    {
        /// <value>
        /// Order's price in base currency.
        /// </value>
        public double Price { get; set; }

        /// <value>
        /// Order's volume (amount) in base currency.
        /// </value>
        public double Volume { get; set; }

        /// <value>
        /// Order's creation date.
        /// </value>
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Class containing asks and bids lists.
    /// </summary>
    /// <see cref="OrderBookEntry"/>
    class OrderBook
    {
        public OrderBookEntry[]? Asks { get; set; }
        public OrderBookEntry[]? Bids { get; set; }
    }

    /// <summary>
    /// Enum containing available order types.
    /// </summary>
    enum OrderType
    {
        Market,
        Limit,
        Instant
    }

    /// <summary>
    /// Class containing basic order data.
    /// </summary>
    /// <see cref="TradingPair"/>
    class Order
    {
        /// <value>
        /// Order ID used by market.
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// Optional client ID, which is included if API is used by more than 
        /// one user.
        /// </value>
        public int? ClientId { get; set; }

        /// <value>
        /// Creation date of current order.
        /// </value>
        public DateTime Date { get; set; }

        /// <value>
        /// Initial volume (amount) of current order.
        /// </value>
        public double InitialVolume { get; set; }

        /// <value>
        /// Remaining volume (amount) of current order.
        /// </value>
        public double RemainingVolume { get; set; }

        /// <value>
        /// Trading pair data for current order.
        /// </value>
        public TradingPair? TradingPair { get; set; }
    }

    /// <summary>
    /// Class <c>WebsocketsToken</c> containing websockets token and expiration 
    /// date.
    /// </summary>
    class WebsocketsToken
    {
        public string? Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    /// <summary>
    /// <c>IMarketApi</c> is a cryptocurrency market API interface used by application 
    /// for all operations. Every implemented method must be asynchronous, hence returns
    /// <c>Task<T></c>.
    /// </summary>
    internal interface IMarketApi
    {
        /// <summary>
        /// Gets all available trading pairs
        /// </summary>
        /// <returns></returns>
        public Task<TradingPair[]> GetTradingPairs();

        /// <summary>
        /// Gets available time intervals for querying OHLC data
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns>Array of time intervals in seconds.</returns>
        public Task<uint[]> GetOhlcTimeIntervals(TradingPair tradingPair);

        /// <summary>
        /// Gets OHLC data for given trading pair, start date and time interval. 
        /// Time interval must be one of intervals returned by <c>GetOhlcTimeIntervals</c>.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs</param>
        /// <param name="startDate"></param>
        /// <param name="timeInterval"></param>
        /// <returns></returns>
        public Task<TickData[]> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval);

        /// <summary>
        /// Gets current tick data for given trading pair.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs</param>
        /// <returns></returns>
        public Task<TickData> GetTick(TradingPair tradingPair);

        /// <summary>
        /// Gets market trading fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs</param>
        /// <returns></returns>
        public Task<Fees[]> GetTradingFees(TradingPair tradingPair);

        /// <summary>
        /// Gets withdrawal fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public Task<Fees[]> GetWithdrawalFees(TradingPair tradingPair);

        /// <summary>
        /// Gets account balance as an array of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public Task<Balance[]> GetAccountBalance();

        /// <summary>
        /// Gets market trading balance as an array of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public Task<Balance[]> GetTradingBalance();

        /// <summary>
        /// Gets order book of asks/bids.
        /// </summary>
        /// <returns></returns>
        public Task<OrderBook> GetOrderBook();

        /// <summary>
        /// Creates buy order with given type and amount (volume) and optional price.
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <param name="price">Used in orders of type <c>OrderType.Limit</c></param>
        /// <returns>ID of created order.</returns>
        public Task<int> BuyOrder(OrderType orderType, double amount, double price = 0);

        /// <summary>
        /// Creates sell order with given type and amount (volume) and optional price.
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <param name="price">Used in orders of type <c>OrderType.Limit</c></param>
        /// <returns>ID of created order.</returns>
        public Task<int> SellOrder(OrderType orderType, double amount, double price = 0);

        /// <summary>
        /// Cancells order of given ID.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public Task<bool> CancelOrder(int orderId);

        /// <summary>
        /// Cancells all user orders.
        /// </summary>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public Task<bool> CancelAllOrders();

        /// <summary>
        /// Gets list of open user orders.
        /// </summary>
        /// <returns></returns>
        public Task<Order[]> GetOpenOrders();

        /// <summary>
        /// Gets list of closed user orders.
        /// </summary>
        /// <returns></returns>
        public Task<Order[]> GetClosedOrders();

        /// <summary>
        /// Gets websockets token and expiration date for real-time websockets 
        /// communication.
        /// </summary>
        /// <returns></returns>
        public Task<WebsocketsToken> GetWebsocketsToken();
    }
}
