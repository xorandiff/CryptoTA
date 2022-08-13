using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    internal class BitstampApi : IMarketApi
    {
        public class BitstampTick
        {
            public double Last { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Vwap { get; set; }
            public double Volume { get; set; }
            public double Bid { get; set; }
            public double Ask { get; set; }
            public double Timestamp { get; set; }
            public double Open { get; set; }
            public double Open_24 { get; set; }
            public float Percent_change_24 { get; set; }

        }

        public class BitstampOhlcItem
        {
            public double High { get; set; }
            public long Timestamp { get; set; }
            public double Volume { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public double Open { get; set; }
        }

        public class BitstampOhlcData
        {
            public string? Pair { get; set; }
            public BitstampOhlcItem[]? Ohlc { get; set; }
        }

        public class BitstampOhlc
        {
            public BitstampOhlcData? Data { get; set; }
        }

        public class BitstampTradingPair
        {
            public string? Trading { get; set; }
            public uint Base_decimals { get; set; }
            public string? Url_symbol { get; set; }
            public string? Name { get; set; }
            public string? Instant_and_market_orders { get; set; }
            public string? Minimum_order { get; set; }
            public uint Counter_decimals { get; set; }
            public string? Description { get; set; }
        }

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

        public async Task<TickData[]> GetOhlcData(TradingPair tradingPair, DateTime startDate, uint timeInterval)
        {
            string uriString = "https://www.bitstamp.net/api/v2/ohlc/";
            uriString       += tradingPair.Name;
            uriString       += "/?limit=1000&step=" + timeInterval;

            Uri baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<BitstampOhlc>(request);

            TickData[] ohlcData = { };

            if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
            {
                foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();

                    var tickData = new TickData
                    {
                        Open = ohlcItem.Open,
                        Close = ohlcItem.Close,
                        High = ohlcItem.High,
                        Low = ohlcItem.Low,
                        Volume = ohlcItem.Volume,
                        Date = dateTime,
                    };
                    _ = ohlcData.Append(tickData);
                }
            }
            
            return ohlcData;
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

        public async Task<TickData> GetTick(TradingPair tradingPair)
        {
            var baseUrl = new Uri($"https://www.bitstamp.net/api/v2/ticker/{tradingPair.Name}/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest("get", Method.Get);

            var response = await client.ExecuteAsync<BitstampTick>(request);

            if (response.IsSuccessful && response.Data != null)
            {
                return new TickData
                {
                    High = response.Data.High,
                    Low = response.Data.Low,
                    Open = response.Data.Open,
                    Close = response.Data.Last,
                    Volume = response.Data.Volume,
                    Date = DateTime.Now
                };
            }
            else
            {
                throw new TaskCanceledException();
            }
        }

        public Task<Balance[]> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<Fees[]> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public async Task<TradingPair[]> GetTradingPairs()
        {
            var baseUrl = new Uri("https://www.bitstamp.net/api/v2/trading-pairs-info/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<BitstampTradingPair>(request);

            TradingPair[] tradingPairs = { };

            if (response.IsSuccessful && response.Data != null && response.Data.Name != null && response.Data.Description != null)
            {
                var pairSymbols = response.Data.Name.Split("/");
                var pairNames = response.Data.Description.Split(" / ");
                var tradingPair = new TradingPair
                {
                    Name = response.Data.Url_symbol,
                    BaseSymbol = pairSymbols[0],
                    CounterSymbol = pairSymbols[1],
                    BaseName = pairNames[0],
                    CounterName = pairNames[1]
                };
                _ = tradingPairs.Append(tradingPair);
            }

            return tradingPairs;
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
