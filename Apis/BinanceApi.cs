using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    public class BinanceApi : IMarketApi
    {
        private readonly string name = "Binance";
        private bool enabled = false;

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

        public Task<int> BuyOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelAllOrders()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Balance>> GetAccountBalance()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TickData>> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<uint>> GetOhlcTimeIntervals(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public Task<OrderBook> GetOrderBook()
        {
            throw new NotImplementedException();
        }

        public Task<TickData> GetTick(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Balance>> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Fees>> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TradingPair>> GetTradingPairs()
        {
            throw new NotImplementedException();
        }

        public Task<WebsocketsToken> GetWebsocketsToken()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Fees>> GetWithdrawalFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<int> SellOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }
    }
}
