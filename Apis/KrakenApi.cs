using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CryptoTA.Apis
{
    public class KrakenApi : IMarketApi
    {
        private readonly string name = "Kraken";
        private readonly uint[] ohlcTimeIntervals = { 60, 300, 900, 1800, 3600, 14400, 86400, 604800, 1296000 };
        private bool enabled = false;

        private readonly string? apiKey = null;
        private readonly string? apiSign = null;

        private readonly RestClient restClient = new RestClient("https://api.kraken.com/0");

        public bool Enabled { get => enabled; set => enabled = value; }
        public string Name { get => name; }

        public uint[] OhlcTimeIntervals { get => ohlcTimeIntervals; }

        public uint OhlcMaxDensityTimeInterval { get => ohlcTimeIntervals.Min() * 720; }

        public class KrakenResult<T>
        {
            public Dictionary<string, T>? Result { get; set; }
            public string[] Error { get; set; }
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

        public class KrakenTickerData
        {
            public string[] A { get; set; }
            public string[] B { get; set; }
            public string[] C { get; set; }
            public string[] V { get; set; }
            public string[] P { get; set; }
            public int[] T { get; set; }
            public string[] L { get; set; }
            public string[] H { get; set; }
            public string O { get; set; }
        }

        private void ResponseErrorCheck(string[] error)
        {
            if (error.Length > 0)
            {
                throw new Exception($"Kraken API response error: {error[0]}");
            }
        }

        private string GetKrakenSignature(string urlPath, ulong nonce, Dictionary<string, string> data)
        {
            var hash256 = SHA256.Create();
            var postData = string.Join("&", data.Select(e => e.Key + "=" + e.Value).ToArray());
            var encoded = Encoding.UTF8.GetBytes(nonce + postData);
            var message = Encoding.UTF8.GetBytes(urlPath).Concat(hash256.ComputeHash(encoded)).ToArray();
            var mac = new HMACSHA512(Convert.FromBase64String(apiSign!));
            return Convert.ToBase64String(mac.ComputeHash(message));
        }

        private RestRequest AuthPostRequest(string uriPath, Dictionary<string, string> data)
        {
            if (apiKey == null || apiSign == null)
            {
                throw new Exception("Method requires credentials.");
            }

            data.Add("nonce", "0");
            var sign = GetKrakenSignature(uriPath, ulong.Parse(data["nonce"]), data);
            return new RestRequest(uriPath)
                                .AddHeader("API-Key", apiKey!)
                                .AddHeader("API-Sign", sign)
                                .AddBody(data);
        }

        public KrakenApi()
        {
            using var db = new DatabaseContext();
            var dbMarket = db.Markets.Include(market => market.Credentials).Where(market => market.Name == Name).FirstOrDefault();
            if (dbMarket != null && dbMarket.Credentials.FirstOrDefault() is Credentials credentials)
            {
                apiKey = credentials.PublicKey;
                apiSign = credentials.PrivateKey;
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
            var data = new Dictionary<string, string> { };
            var response = await restClient.PostAsync<KrakenResult<string>>(AuthPostRequest("private/Balance", data));

            if (response == null)
            {
                throw new Exception("Error during parsing Kraken account balance response.");
            }
            
            ResponseErrorCheck(response.Error);

            var balance = new List<Balance>();

            foreach (var kvp in response.Result!)
            {
                balance.Add(new Balance
                {
                    Name = kvp.Key,
                    TotalAmount = double.Parse(kvp.Value, CultureInfo.InvariantCulture)
                });
            }

            return balance;
        }

        public Task<List<Order>> GetClosedOrders()
        {
            throw new NotImplementedException();
        }

        public List<Tick> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            var request = new RestRequest("public/OHLC")
                            .AddQueryParameter("pair", tradingPair.Name)
                            .AddQueryParameter("interval", timeInterval);
            if (startDate != null)
            {
                var startTimestamp = new DateTimeOffset((DateTime)startDate).ToUnixTimeSeconds();
                request.AddQueryParameter("since", startTimestamp);
            }

            var response = restClient.Execute(request);
            var ohlcData = new List<Tick>();

            if (response is not RestResponse { Content: string content } _ || JObject.Parse(content) is not JObject responseJson)
            {
                throw new Exception("Too many requests error returned from Kraken API.");
            }

            if (responseJson["error"] is JObject error && error.Count > 0)
            {
                ResponseErrorCheck(new string[] { error[0]!.ToString() });
            }

            if (responseJson["result"] is null || responseJson["result"]![tradingPair.Name] is null)
            {
                //throw new Exception("Error during parsing Kraken OHLC Tick.");
                return ohlcData;
            }

            foreach (var krakenOhlcTick in responseJson["result"]![tradingPair.Name]!.AsJEnumerable())
            {
                ohlcData.Add(new Tick
                {
                    Date = DateTimeUtils.FromTimestamp((int)krakenOhlcTick[0]!),
                    Open = double.Parse((string)krakenOhlcTick[1]!, CultureInfo.InvariantCulture),
                    High = double.Parse((string)krakenOhlcTick[2]!, CultureInfo.InvariantCulture),
                    Low = double.Parse((string)krakenOhlcTick[3]!, CultureInfo.InvariantCulture),
                    Close = double.Parse((string)krakenOhlcTick[4]!, CultureInfo.InvariantCulture),
                    Volume = double.Parse((string)krakenOhlcTick[6]!, CultureInfo.InvariantCulture),
                });
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

        public async Task<Tick?> GetTick(TradingPair tradingPair)
        {
            var request = new RestRequest("public/Ticker").AddQueryParameter("pair", tradingPair.Name);

            var response = await restClient.GetAsync<KrakenResult<KrakenTickerData>>(request);
            if (response == null)
            {
                throw new Exception("Error parsing Kraken ticker data");
            }

            ResponseErrorCheck(response.Error);

            KrakenTickerData krakenTickerData = response.Result!.First().Value;

            return new Tick
            {
                High = double.Parse(krakenTickerData.H[0], CultureInfo.InvariantCulture),
                Low = double.Parse(krakenTickerData.L[0], CultureInfo.InvariantCulture),
                Open = double.Parse(krakenTickerData.O, CultureInfo.InvariantCulture),
                Close = double.Parse(krakenTickerData.C[0], CultureInfo.InvariantCulture),
                Volume = double.Parse(krakenTickerData.V[0], CultureInfo.InvariantCulture),
                Date = DateTime.Now
            };
        }

        public Task<List<Balance>> GetTradingBalance()
        {
            throw new NotImplementedException();
        }

        public Task<List<Fees>> GetTradingFees(TradingPair tradingPair)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TradingPair>> GetTradingPairs()
        {
            var tradingPairs = new List<TradingPair>();

            var response = await restClient.GetJsonAsync<KrakenResult<KrakenTradingPair>>("public/AssetPairs");
            if (response == null)
            {
                throw new Exception("Couldn't parse Kraken API trading pairs.");
            }

            ResponseErrorCheck(response.Error);

            foreach (var tradingPairKvp in response.Result!)
            {
                var krakenTradingPair = tradingPairKvp.Value;

                if (krakenTradingPair.Wsname is not string wsname)
                {
                    throw new Exception("Kraken trading pair has no wsname property.");
                }
                var pairSymbols = krakenTradingPair.Wsname.Split("/");

                tradingPairs.Add(new TradingPair
                {
                    Name = tradingPairKvp.Key,
                    AlternativeName = krakenTradingPair.Altname,
                    WebsocketName = krakenTradingPair.Wsname,
                    BaseSymbol = pairSymbols[0],
                    CounterSymbol = pairSymbols[1],
                    BaseName = pairSymbols[0],
                    CounterName = pairSymbols[1],
                    MinimalOrderAmount = krakenTradingPair.Ordermin != null ? double.Parse(krakenTradingPair.Ordermin, CultureInfo.InvariantCulture) : 0,
                });
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
