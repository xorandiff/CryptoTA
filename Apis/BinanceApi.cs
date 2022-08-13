﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTA.Models;

namespace CryptoTA.Apis
{
    public class BinanceApi : IMarketApi
    {
        private readonly string name = "Binance";
        private readonly uint[] ohlcTimeIntervals = { };
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

        public uint[] OhlcTimeIntervals
        {
            get
            {
                return ohlcTimeIntervals;
            }
        }

        public uint OhlcMaxDensityTimeInterval => throw new NotImplementedException();

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

        public Task<List<Balance>> GetAccountBalance()
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public Task<List<TickData>> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOpenOrders()
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

        public Task<List<Balance>> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<List<TradingPair>> GetTradingPairs()
        {
            throw new NotImplementedException();
        }

        public Task<WebsocketsToken> GetWebsocketsToken()
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetWithdrawalFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public Task<int> SellOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }
    }
}
