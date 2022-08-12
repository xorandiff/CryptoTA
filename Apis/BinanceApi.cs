using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    internal class BinanceApi : IMarketApi
    {
        public Task<int> BuyOrder(OrderType orderType, double amount, double price = 0)
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

        public Task<Balance[]> GetAccountBalance()
        {
            throw new NotImplementedException();
        }

        public Task<Order[]> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public Task<TickData[]> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        public Task<uint[]> GetOhlcTimeIntervals(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<Order[]> GetOpenOrders()
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

        public Task<Balance[]> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<Fees[]> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<TradingPair[]> GetTradingPairs()
        {
            throw new NotImplementedException();
        }

        public Task<WebsocketsToken> GetWebsocketsToken()
        {
            throw new NotImplementedException();
        }

        public Task<Fees[]> GetWithdrawalFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<int> SellOrder(OrderType orderType, double amount, double price = 0)
        {
            throw new NotImplementedException();
        }
    }
}
