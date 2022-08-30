using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    /// <summary>
    /// <c>IMarketApi</c> is a cryptocurrency market API interface used by application 
    /// for all operations. Every implemented method must be asynchronous, hence returns
    /// <c>Task<T></c>.
    /// </summary>
    public interface IMarketApi
    {
        /// <value>
        /// Boolean property for enabling/disabling current API 
        /// in the application.
        /// </value>
        public bool Enabled { get; set; }

        /// <value>
        /// Displayed market name.
        /// </value>
        public string Name { get; }

        /// <value>
        /// Available time intervals (in seconds) for querying OHLC data.
        /// </value>
        public uint[] OhlcTimeIntervals { get; }

        /// <value>
        /// Maxmial amount of ticks per OHLC request.
        /// </value>
        public uint RequestMaxTickCount { get; }

        /// <value>
        /// Maxmial amount of secods of densiest possible time interval for single 
        /// OHLC query.
        /// </value>
        public uint OhlcMaxDensityTimeInterval { get; }

        /// <summary>
        /// Gets all available trading pairs.
        /// </summary>
        /// <returns></returns>
        public Task<List<TradingPair>> GetTradingPairs();

        /// <summary>
        /// Gets market asset data.
        /// </summary>
        /// <returns></returns>
        public Asset GetAssetData(string assetName);

        /// <summary>
        /// Async version of <c>GetAssetData</c>.
        /// </summary>
        /// <returns></returns>
        public Task<Asset> GetAssetDataAsync(string assetName);

        /// <summary>
        /// Gets OHLC data for given trading pair, start date and time interval. 
        /// Time interval must be one of intervals from property <c>OhlcTimeIntervals</c>.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs</param>
        /// <param name="startDate"></param>
        /// <param name="timeInterval"></param>
        /// <returns></returns>
        public List<Tick> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval);

        /// <summary>
        /// Gets current tick data for given trading pair.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs.</param>
        /// <returns></returns>
        public Task<Tick?> GetTick(TradingPair tradingPair);

        /// <summary>
        /// Gets market trading fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs.</param>
        /// <returns></returns>
        public Task<List<Fees>> GetTradingFees(TradingPair tradingPair);

        /// <summary>
        /// Gets withdrawal fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public Task<List<Fees>> GetWithdrawalFees(TradingPair tradingPair);

        /// <summary>
        /// Gets account balance as an array of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public List<Balance> GetAccountBalance();

        /// <summary>
        /// Async version of <c>GetAccountBalance</c>
        /// </summary>
        /// <returns></returns>
        public Task<List<Balance>> GetAccountBalanceAsync();

        /// <summary>
        /// Gets market trading balance as a enumerable of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public Task<List<Balance>> GetTradingBalance();

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
        public Task<List<Order>> GetOpenOrders();

        /// <summary>
        /// Gets list of closed user orders.
        /// </summary>
        /// <returns></returns>
        public Task<List<Order>> GetClosedOrders();

        /// <summary>
        /// Gets list of account's trades.
        /// </summary>
        /// <returns></returns>
        public Task<List<Trade>> GetTradesHistory();

        /// <summary>
        /// Gets websockets token and expiration date for real-time websockets 
        /// communication.
        /// </summary>
        /// <returns></returns>
        public Task<WebsocketsToken> GetWebsocketsToken();
    }
}
