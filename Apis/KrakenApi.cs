using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    internal class KrakenApi : IMarketApi
    {
        private readonly string name = "Kraken";
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

        Task<int> IMarketApi.BuyOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }

        Task<bool> IMarketApi.CancelAllOrders()
        {
            throw new NotImplementedException();
        }

        Task<bool> IMarketApi.CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Balance>> IMarketApi.GetAccountBalance()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Order>> IMarketApi.GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TickData>> IMarketApi.GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<uint>> IMarketApi.GetOhlcTimeIntervals(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Order>> IMarketApi.GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        Task<OrderBook> IMarketApi.GetOrderBook()
        {
            throw new NotImplementedException();
        }

        Task<TickData> IMarketApi.GetTick(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Balance>> IMarketApi.GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Fees>> IMarketApi.GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TradingPair>> IMarketApi.GetTradingPairs()
        {
            throw new NotImplementedException();
        }

        Task<WebsocketsToken> IMarketApi.GetWebsocketsToken()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Fees>> IMarketApi.GetWithdrawalFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        Task<int> IMarketApi.SellOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }
    }
}
