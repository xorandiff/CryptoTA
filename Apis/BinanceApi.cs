using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    public class BinanceApi : IMarketApi
    {
        private readonly string name = "Binance";
        private readonly uint[] ohlcTimeIntervals = { };
        private bool enabled = false;
        private const uint requestMaxTickCount = 0;

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

        public uint[] OhlcTimeIntervals
        {
            get
            {
                return ohlcTimeIntervals;
            }
        }

        public uint RequestMaxTickCount
        {
            get
            {
                return requestMaxTickCount;
            }
        }

        public uint OhlcMaxDensityTimeInterval => throw new NotImplementedException();

        public List<Asset> GetAssets()
        {
            throw new NotImplementedException();
        }

        public List<TradingPair> GetTradingPairs()
        {
            throw new NotImplementedException();
        }

        public Asset GetAssetData(string assetName)
        {
            throw new NotImplementedException();
        }

        public List<Tick> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        public Tick? GetTick(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public List<Fees> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public List<Fees> GetWithdrawalFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public List<Balance> GetAccountBalance()
        {
            throw new NotImplementedException();
        }

        public List<Balance> GetAccountBalance(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public List<Balance> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public OrderBook GetOrderBook(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public string BuyOrder(TradingPair tradingPair, OrderType orderType, double amount)
        {
            throw new NotImplementedException();
        }

        public string SellOrder(TradingPair tradingPair, OrderType orderType, double amount)
        {
            throw new NotImplementedException();
        }

        public bool CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        public bool CancelAllOrders()
        {
            throw new NotImplementedException();
        }

        public List<Order> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public List<Order> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public List<Trade> GetTradesHistory()
        {
            throw new NotImplementedException();
        }

        public WebsocketsToken GetWebsocketsToken()
        {
            throw new NotImplementedException();
        }

        public List<Ledger> GetLedgers()
        {
            throw new NotImplementedException();
        }

        public Task<List<Asset>> GetAssetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<TradingPair>> GetTradingPairsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Asset> GetAssetDataAsync(string assetName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Tick>> GetOhlcDataAsync(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        public Task<Tick?> GetTickAsync(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetTradingFeesAsync(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetWithdrawalFeesAsync(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<List<Balance>> GetAccountBalanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Balance>> GetAccountBalanceAsync(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<List<Balance>> GetTradingBalanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OrderBook> GetOrderBookAsync(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<string> BuyOrderAsync(TradingPair tradingPair, OrderType orderType, double amount)
        {
            throw new NotImplementedException();
        }

        public Task<string> SellOrderAsync(TradingPair tradingPair, OrderType orderType, double amount)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelAllOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOpenOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetClosedOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Trade>> GetTradesHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<WebsocketsToken> GetWebsocketsTokenAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Ledger>> GetLedgersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
