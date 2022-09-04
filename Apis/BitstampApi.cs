using CryptoTA.Database.Models;
using RestSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoTA.Apis
{
    public class BitstampApi : IMarketApi
    {
        private const string name = "Bitstamp";
        private readonly uint[] ohlcTimeIntervals = { 60, 180, 300, 900, 1800, 3600, 7200, 14400, 21600, 43200, 86400, 259200 };
        private bool enabled = false;
        private const uint requestMaxTickCount = 1000;

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

        public uint OhlcMaxDensityTimeInterval
        {
            get
            {
                return ohlcTimeIntervals.Min() * 1000;
            }
        }

        public uint RequestMaxTickCount
        {
            get
            {
                return requestMaxTickCount;
            }
        }

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

        public List<Tick> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            string uriString = "https://www.bitstamp.net/api/v2/ohlc/";
            uriString       += tradingPair.Name;
            uriString       += "/?limit=1000&step=" + timeInterval;
            if (startDate != null)
            {
                var startTimestamp = new DateTimeOffset((DateTime)startDate).ToUnixTimeSeconds();
                uriString += "&start=" + startTimestamp;
            }

            var baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = client.Execute<BitstampOhlc>(request);

            var ohlcData = new List<Tick>();

            if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
            {
                foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
                {
                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();

                    var Tick = new Tick
                    {
                        Open = ohlcItem.Open,
                        Close = ohlcItem.Close,
                        High = ohlcItem.High,
                        Low = ohlcItem.Low,
                        Volume = ohlcItem.Volume,
                        Date = dateTime,
                    };
                    ohlcData.Add(Tick);
                }
            }
            
            return ohlcData;
        }

        public async Task<List<Tick>> GetOhlcDataAsync(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            string uriString = "https://www.bitstamp.net/api/v2/ohlc/";
            uriString += tradingPair.Name;
            uriString += "/?limit=1000&step=" + timeInterval;
            if (startDate != null)
            {
                var startTimestamp = new DateTimeOffset((DateTime)startDate).ToUnixTimeSeconds();
                uriString += "&start=" + startTimestamp;
            }

            var baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<BitstampOhlc>(request);

            var ohlcData = new List<Tick>();

            if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
            {
                foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
                {
                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();

                    var Tick = new Tick
                    {
                        Open = ohlcItem.Open,
                        Close = ohlcItem.Close,
                        High = ohlcItem.High,
                        Low = ohlcItem.Low,
                        Volume = ohlcItem.Volume,
                        Date = dateTime,
                    };
                    ohlcData.Add(Tick);
                }
            }

            return ohlcData;
        }

        public Tick? GetTick(TradingPair tradingPair)
        {
            var baseUrl = new Uri($"https://www.bitstamp.net/api/v2/ticker/{tradingPair.Name}/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest("get", Method.Get);

            var response = client.Execute<BitstampTick>(request);

            if (response.IsSuccessful && response.Data != null)
            {
                return new Tick
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
                return null;
            }
        }

        public async Task<Tick?> GetTickAsync(TradingPair tradingPair)
        {
            var baseUrl = new Uri($"https://www.bitstamp.net/api/v2/ticker/{tradingPair.Name}/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest("get", Method.Get);

            var response = await client.ExecuteAsync<BitstampTick>(request);

            if (response.IsSuccessful && response.Data != null)
            {
                return new Tick
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
                return null;
            }
        }

        public List<TradingPair> GetTradingPairs(string[]? tradingPairNames = null)
        {
            var baseUrl = new Uri("https://www.bitstamp.net/api/v2/trading-pairs-info/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = client.Execute<BitstampTradingPair[]>(request);

            var tradingPairs = new List<TradingPair>();

            if (response.IsSuccessful && response.Data != null)
            {
                if (response.Data.Length == 0)
                {
                    throw new Exception("API call successful, but no trading pairs serialized.");
                }

                foreach (var bitstampTradingPair in response.Data)
                {
                    if (bitstampTradingPair.Name != null && bitstampTradingPair.Description != null)
                    {
                        var pairSymbols = bitstampTradingPair.Name.Split("/");
                        var pairNames = bitstampTradingPair.Description.Split(" / ");
                        var tradingPair = new TradingPair
                        {
                            Name = bitstampTradingPair.Url_symbol,
                            BaseSymbol = pairSymbols[0],
                            CounterSymbol = pairSymbols[1],
                            BaseName = pairNames[0],
                            CounterName = pairNames[1]
                        };
                        tradingPairs.Add(tradingPair);
                    }
                }

                return tradingPairs;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public async Task<List<TradingPair>> GetTradingPairsAsync(string[]? tradingPairNames = null)
        {
            var baseUrl = new Uri("https://www.bitstamp.net/api/v2/trading-pairs-info/");
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<BitstampTradingPair[]>(request);

            var tradingPairs = new List<TradingPair>();

            if (response.IsSuccessful && response.Data != null)
            {
                if (response.Data.Length == 0)
                {
                    throw new Exception("API call successful, but no trading pairs serialized.");
                }

                foreach (var bitstampTradingPair in response.Data)
                {
                    if (bitstampTradingPair.Name != null && bitstampTradingPair.Description != null)
                    {
                        var pairSymbols = bitstampTradingPair.Name.Split("/");
                        var pairNames = bitstampTradingPair.Description.Split(" / ");
                        var tradingPair = new TradingPair
                        {
                            Name = bitstampTradingPair.Url_symbol,
                            BaseSymbol = pairSymbols[0],
                            CounterSymbol = pairSymbols[1],
                            BaseName = pairNames[0],
                            CounterName = pairNames[1]
                        };
                        tradingPairs.Add(tradingPair);
                    }
                }

                return tradingPairs;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public List<Asset> GetAssets(string[]? assetNames = null)
        {
            throw new NotImplementedException();
        }

        public Fees GetTradingFees(TradingPair tradingPair, double baseVolume)
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

        public string BuyOrder(TradingPair tradingPair, OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }

        public string SellOrder(TradingPair tradingPair, OrderType orderType, double amount, double price)
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

        public Task<List<Asset>> GetAssetsAsync(string[]? assetNames = null)
        {
            throw new NotImplementedException();
        }

        public Task<Fees> GetTradingFeesAsync(TradingPair tradingPair, double baseVolume)
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

        public List<Order> GetOrdersInfo(string[] transactionIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<Order>> GetOrdersInfoAsync(string[] transactionIds)
        {
            throw new NotImplementedException();
        }
    }
}
