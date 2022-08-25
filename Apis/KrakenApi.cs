using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace CryptoTA.Apis
{
    public class KrakenApi : IMarketApi
    {
        private readonly string name = "Kraken";
        private readonly uint[] ohlcTimeIntervals = { 60, 300, 900, 1800, 3600, 14400, 86400, 604800, 1296000 };
        private bool enabled = false;

        private string? apiKey = null;
        private string? apiSign = null;

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
                return ohlcTimeIntervals.Min() * 720;
            }
        }

        public class KrakenResult
        {
            public Dictionary<string, object> Result { get; set; }
        }

        public class KrakenAssetInfo
        {
            public string? Name { get; set; }
            public string? Aclass { get; set; }
            public string? Altname { get; set; }
            public uint Decimals { get; set; }
            public uint Display_decimals { get; set; }
            public double? Collateral_value { get; set; }
        }

        public class KrakenTradingPair
        {
            public string? Name { get; set; }
            public string? Altname { get; set; }
            public string? Wsname { get; set; }
            public string? Aclass_base { get; set; }
            public string? Base { get; set; }
            public string? Aclass_quote { get; set; }
            public string? Quote { get; set; }
            public string? Lot { get; set; }
            public uint Decimals { get; set; }
            public uint Pair_decimals { get; set; }
            public uint Lot_decimals { get; set; }
            public uint Lot_multiplier { get; set; }
            public string? Fee_volume_currency { get; set; }
            public uint Margin_call { get; set; }
            public uint Margin_stop { get; set; }
            public string? Ordermin { get; set; }
        }

        public class KrakenTick
        {
            public string[] A { get; set; }
            public string[] B { get; set; }
            public string[] C { get; set; }
            public string[] V { get; set; }
            public string[] P { get; set; }
            public string[] T { get; set; }
            public string[] L { get; set; }
            public string[] H { get; set; }
            public string[] O { get; set; }
        }

        public KrakenApi()
        {
            using (var db = new DatabaseContext())
            {
                var dbMarket = db.Markets.Include(market => market.Credentials).Where(market => market.Name == Name).FirstOrDefault();
                if (dbMarket != null && dbMarket.Credentials != null)
                {
                    apiKey = dbMarket.Credentials.FirstOrDefault()!.PublicKey;
                    apiSign = dbMarket.Credentials.FirstOrDefault()!.PrivateKey;
                }
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

        public async Task<List<Balance>> GetAccountBalance()
        {
            if (apiKey == null || apiSign == null)
            {
                throw new Exception("GetAccountBalance() requires credentials.");
            }

            var baseUrl = new Uri("https://api.kraken.com/0/private/Balance");
            var client = new RestClient(baseUrl);
            var request = new RestRequest("post", Method.Post);

            request.AddHeader("API-Key", apiKey);
            request.AddHeader("API-Sign", apiSign);

            var response = await client.ExecuteAsync<KrakenResult>(request);

            if (response.IsSuccessful && response.Data != null && response.Data.Result != null)
            {
                var balance = new List<Balance>();
                var krakenResultDictionary = response.Data.Result;

                foreach (var kvp in krakenResultDictionary)
                {
                    balance.Add(new Balance
                    {
                        Name = kvp.Key,
                        TotalAmount = (double) kvp.Value
                    });
                }

                return balance;
            }
            else
            {
                throw new TaskCanceledException();
            }
        }

        public Task<List<Order>> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Tick>> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            string uriString = "https://api.kraken.com/0/public/OHLC?pair=";
            uriString += tradingPair.Name;
            uriString += "&interval=" + (timeInterval / 60);
            if (startDate != null)
            {
                var startTimestamp = new DateTimeOffset((DateTime)startDate).ToUnixTimeSeconds();
                uriString += "&since=" + startTimestamp;
            }

            var baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<KrakenResult>(request);

            var ohlcData = new List<Tick>();

            if (response.IsSuccessful && response.Data != null && response.Data.Result != null)
            {
                foreach (object[] ohlcItem in (object[,])response.Data.Result.GetType().GetProperty(tradingPair.Name)!.GetValue(response.Data.Result)!)
                {
                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds((uint) ohlcItem[0]).ToLocalTime();

                    var Tick = new Tick
                    {
                        Open = (double) ohlcItem[1],
                        Close = (double)ohlcItem[4],
                        High = (double)ohlcItem[2],
                        Low = (double)ohlcItem[3],
                        Volume = (double)ohlcItem[6],
                        Date = dateTime,
                    };
                    ohlcData.Add(Tick);
                }
            }

            return ohlcData;
        }

        public Task<List<Order>> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public Task<OrderBook> GetOrderBook()
        {
            throw new NotImplementedException();
        }

        public async Task<Tick> GetTick(TradingPair tradingPair)
        {
            var baseUrl = new Uri("https://api.kraken.com/0/public/Ticker");
            var client = new RestClient(baseUrl);
            var request = new RestRequest().AddQueryParameter("pair", tradingPair.Name);

            var response = await client.GetAsync<KrakenResult>(request);

            if (response != null && response.Result != null)
            {
                Dictionary<string, object> resultDictrionary = response.Result;

                if (resultDictrionary.Keys.Count == 0)
                {
                    throw new Exception("API call was successful, but no Kraken assets data were serialized.");
                }

                var tick = new Tick { Date = DateTime.Now };

                foreach (var kvp in resultDictrionary)
                {
                    if (kvp.Key == "h")
                    {
                        tick.High = double.Parse(((string[])kvp.Value)[0]);
                    }
                    if (kvp.Key == "l")
                    {
                        tick.Low = double.Parse(((string[])kvp.Value)[0]);
                    }
                    if (kvp.Key == "o")
                    {
                        tick.Open = double.Parse(((string[])kvp.Value)[0]);
                    }
                    if (kvp.Key == "c")
                    {
                        tick.Close = double.Parse(((string[])kvp.Value)[0]);
                    }
                    if (kvp.Key == "v")
                    {
                        tick.Volume = double.Parse(((string[])kvp.Value)[0]);
                    }
                }

                return tick;
            }
            else
            {
                throw new TaskCanceledException();
            }
        }

        public Task<List<Balance>> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        private async Task<List<KrakenAssetInfo>> GetAssetsInfo()
        {
            var baseUrl = new Uri("https://api.kraken.com/0/public/Assets");
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<KrakenResult>(request);

            var assetsInfo = new List<KrakenAssetInfo>();

            if (response.IsSuccessful && response.Data != null && response.Data.Result != null)
            {
                var krakenResultDictionary = response.Data.Result; 

                if (krakenResultDictionary.Keys.Count == 0)
                {
                    throw new Exception("API call was successful, but no Kraken assets data were serialized.");
                }

                foreach (var assetKvp in krakenResultDictionary)
                {
                    
                    assetsInfo.Add(new KrakenAssetInfo
                    {
                        Name = assetKvp.Key,


                    });
                }

                return assetsInfo;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public async Task<List<TradingPair>> GetTradingPairs()
        {
            var tradingPairs = new List<TradingPair>();
            var assets = await GetAssetsInfo();

            var realCurrencies = new List<string> { "ZUSD", "ZEUR", "ZGBP" };

            foreach (var asset in assets)
            {
                var queryTradingPairs = string.Join(",", realCurrencies.Select(s => asset.Name + s).ToList());

                var baseUrl = new Uri("https://api.kraken.com/0/public/AssetPairs?pair=" + queryTradingPairs);
                var client = new RestClient(baseUrl);
                var request = new RestRequest(baseUrl, Method.Get);

                var response = await client.ExecuteAsync<KrakenResult>(request);

                if (response.IsSuccessful && response.Data != null && response.Data.Result != null)
                {
                    var krakenResultDictionary = response.Data.Result;

                    if (krakenResultDictionary.Keys.Count == 0)
                    {
                        throw new Exception("API call was successful, but no Kraken trading pairs were serialized.");
                    }

                    foreach (var tradingPairKvp in krakenResultDictionary)
                    {
                        KrakenTradingPair krakenTradingPair = (KrakenTradingPair)tradingPairKvp.Value;

                        if (krakenTradingPair.Name != null)
                        {
                            var pairSymbols = krakenTradingPair.Wsname!.Split("/");
                            var tradingPair = new TradingPair
                            {
                                Name = tradingPairKvp.Key,
                                BaseSymbol = pairSymbols[0],
                                CounterSymbol = pairSymbols[1],
                                BaseName = pairSymbols[0],
                                CounterName = pairSymbols[1]
                            };
                            tradingPairs.Add(tradingPair);
                        }
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }
            }

            return tradingPairs;
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
