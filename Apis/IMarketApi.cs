using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    /// <summary>
    /// <c>IMarketApi</c> is a cryptocurrency market API interface used by application 
    /// for all operations. Every market API must implement every property and method.
    /// 
    /// Methods are synchronious and have asynchronious equivalents 
    /// with "Async" added at the end of methods's name. Also, if synchronious method 
    /// returns <c>T</c>, then asynchronious equivalent returns <c>Task<T></c>.
    /// </summary>
    public interface IMarketApi
    {
        /***************************************************************
         ************************* Properties **************************
         ***************************************************************/

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

        /***************************************************************
         ********************* Synchronious methods ********************
         ***************************************************************/

        /// <summary>
        /// Gets assets with given names. If no names are given, 
        /// it gets all assets.
        /// </summary>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public List<Asset> GetAssets(string[]? assetNames = null);

        /// <summary>
        /// Gets trading pairs with given names. If no names are given, 
        /// it gets all trading pairs.
        /// </summary>
        /// <param name="tradingPairNames"></param>
        /// <returns></returns>
        public List<TradingPair> GetTradingPairs(string[]? tradingPairNames = null);

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
        public Tick? GetTick(TradingPair tradingPair);

        /// <summary>
        /// Gets market trading fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs.</param>
        /// <returns></returns>
        public List<Fees> GetTradingFees(TradingPair tradingPair);

        /// <summary>
        /// Gets withdrawal fees for given trading pair.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public List<Fees> GetWithdrawalFees(TradingPair tradingPair);

        /// <summary>
        /// Gets account balance as a list of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public List<Balance> GetAccountBalance();

        /// <summary>
        /// Gets subset account balance assets as a list of currency-amount 
        /// <c>Balance</c> objects which matches with given trading pair.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public List<Balance> GetAccountBalance(TradingPair tradingPair);

        /// <summary>
        /// Gets market trading balance as a enumerable of currency-amount <c>Balance</c> 
        /// objects.
        /// </summary>
        /// <returns></returns>
        public List<Balance> GetTradingBalance();

        /// <summary>
        /// Gets order book of asks/bids for given trading pair.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public OrderBook GetOrderBook(TradingPair tradingPair);

        /// <summary>
        /// Creates buy order with given type and amount (volume) and optional price.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <returns>ID of created order.</returns>
        public string BuyOrder(TradingPair tradingPair, OrderType orderType, double amount, double price);

        /// <summary>
        /// Creates sell order with given type and amount (volume) and optional price.
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <param name="price">Used in orders of type <c>OrderType.Limit</c></param>
        /// <returns>ID of created order.</returns>
        public string SellOrder(TradingPair tradingPair, OrderType orderType, double amount, double price);

        /// <summary>
        /// Cancells order of given ID.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public bool CancelOrder(int orderId);

        /// <summary>
        /// Cancells all user's orders.
        /// </summary>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public bool CancelAllOrders();

        /// <summary>
        /// Gets list of open user orders.
        /// </summary>
        /// <returns></returns>
        public List<Order> GetOpenOrders();

        /// <summary>
        /// Gets list of closed user orders.
        /// </summary>
        /// <returns></returns>
        public List<Order> GetClosedOrders();

        /// <summary>
        /// Gets list of account's trades.
        /// </summary>
        /// <returns></returns>
        public List<Trade> GetTradesHistory();

        /// <summary>
        /// Gets websockets token and expiration date for real-time websockets 
        /// communication.
        /// </summary>
        /// <returns></returns>
        public WebsocketsToken GetWebsocketsToken();

        /// <summary>
        /// Gets list of ledgers.
        /// </summary>
        /// <returns></returns>
        public List<Ledger> GetLedgers();

        /***************************************************************
         ****************** Asynchronious equivalents ******************
         ***************************************************************/

        /// <summary>
        /// Async version of <c>GetAssets</c>.
        /// </summary>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public Task<List<Asset>> GetAssetsAsync(string[]? assetNames = null);

        /// <summary>
        /// Async version of <c>GetAssets</c>.
        /// </summary>
        /// <param name="tradingPairNames"></param>
        /// <returns></returns>
        public Task<List<TradingPair>> GetTradingPairsAsync(string[]? tradingPairNames = null);

        /// <summary>
        /// Async version of <c>GetOhlcData</c>.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs</param>
        /// <param name="startDate"></param>
        /// <param name="timeInterval"></param>
        /// <returns></returns>
        public Task<List<Tick>> GetOhlcDataAsync(TradingPair tradingPair, DateTime? startDate, uint timeInterval);

        /// <summary>
        /// Async version of <c>GetTick</c>.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs.</param>
        /// <returns></returns>
        public Task<Tick?> GetTickAsync(TradingPair tradingPair);

        /// <summary>
        /// Async version of <c>GetTradingFees</c>.
        /// </summary>
        /// <param name="tradingPair">One of available trading pairs.</param>
        /// <returns></returns>
        public Task<List<Fees>> GetTradingFeesAsync(TradingPair tradingPair);

        /// <summary>
        /// Async version of <c>GetWithdrawalFees</c>.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public Task<List<Fees>> GetWithdrawalFeesAsync(TradingPair tradingPair);

        /// <summary>
        /// Async version of <c>GetAccountBalance</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Balance>> GetAccountBalanceAsync();

        /// <summary>
        /// Async version of <c>GetAccountBalance</c>.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public Task<List<Balance>> GetAccountBalanceAsync(TradingPair tradingPair);

        /// <summary>
        /// Async version of <c>GetTradingBalance</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Balance>> GetTradingBalanceAsync();

        /// <summary>
        /// Async version of <c>GetOrderBook</c>.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public Task<OrderBook> GetOrderBookAsync(TradingPair tradingPair);

        /// <summary>
        /// Async version of <c>BuyOrder</c>.
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <returns>ID of created order.</returns>
        public Task<string> BuyOrderAsync(TradingPair tradingPair, OrderType orderType, double amount);

        /// <summary>
        /// Async version of <c>SellOrder</c>.
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="amount"></param>
        /// <param name="price">Used in orders of type <c>OrderType.Limit</c></param>
        /// <returns>ID of created order.</returns>
        public Task<string> SellOrderAsync(TradingPair tradingPair, OrderType orderType, double amount);

        /// <summary>
        /// Async version of <c>CancelOrder</c>.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public Task<bool> CancelOrderAsync(int orderId);

        /// <summary>
        /// Async version of <c>CancelAllOrders</c>.
        /// </summary>
        /// <returns>Boolean true/false whether operation succeded/failed.</returns>
        public Task<bool> CancelAllOrdersAsync();

        /// <summary>
        /// Async version of <c>CancelAllOrders</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Order>> GetOpenOrdersAsync();

        /// <summary>
        /// Async version of <c>GetOpenOrders</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Order>> GetClosedOrdersAsync();

        /// <summary>
        /// Async version of <c>GetClosedOrders</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Trade>> GetTradesHistoryAsync();

        /// <summary>
        /// Async version of <c>GetTradesHistory</c>.
        /// </summary>
        /// <returns></returns>
        public Task<WebsocketsToken> GetWebsocketsTokenAsync();

        /// <summary>
        /// Async version of <c>GetWebsocketsToken</c>.
        /// </summary>
        /// <returns></returns>
        public Task<List<Ledger>> GetLedgersAsync();
    }
}
