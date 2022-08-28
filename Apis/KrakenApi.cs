using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CryptoTA.Apis
{
    public class KrakenApi : IMarketApi
    {
        private readonly string name = "Kraken";
        private readonly uint[] ohlcTimeIntervals = { 60, 300, 900, 1800, 3600, 14400, 86400, 604800, 1296000 };
        private bool enabled = false;

        private static string baseDomain = "https://api.kraken.com";
        private static string publicPath = "/0/private/";
        private static string privatePath = "/0/private/";
        private readonly Credentials credentials = new();

        private readonly RestClient restClient = new(baseDomain);

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
         
        public string CreateAuthenticationSignature(string apiPath, string endpointName, string nonce, string inputParams)
        {
            byte[] sha256Hash = ComputeSha256Hash(nonce, inputParams);
            byte[] sha512Hash = ComputeSha512Hash(credentials.PrivateKey, sha256Hash, apiPath, endpointName);
            string signatureString = Convert.ToBase64String(sha512Hash);

            return signatureString;
        }

        private static byte[] ComputeSha256Hash(string nonce, string inputParams)
        {
            byte[] sha256Hash;

            string sha256HashData = nonce.ToString() + "nonce=" + nonce.ToString() + inputParams;

            using (var sha = SHA256.Create())
            {
                sha256Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sha256HashData));
            }

            return sha256Hash;
        }

        private static byte[] ComputeSha512Hash(string apiPrivateKey, byte[] sha256Hash, string apiPath, string endpointName)
        {

            string apiEndpointPath = apiPath + endpointName;

            byte[] apiEndpointPathBytes = Encoding.UTF8.GetBytes(apiEndpointPath);
            byte[] sha512HashData = apiEndpointPathBytes.Concat(sha256Hash).ToArray();
            HMACSHA512 encryptor = new (Convert.FromBase64String(apiPrivateKey));
            byte[] sha512Hash = encryptor.ComputeHash(sha512HashData);

            return sha512Hash;
        }

        private async Task<string> QueryPrivateEndpoint(string endpointName, string inputParameters = "")
        {
            string apiEndpointFullURL = baseDomain + privatePath + endpointName;
            string nonce = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            if (string.IsNullOrWhiteSpace(inputParameters) == false)
            {
                inputParameters = "&" + inputParameters;
            }

            string apiPostBodyData = "nonce=" + nonce + inputParameters;
            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce, inputParameters);
            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", credentials.PublicKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");
                StringContent data = new (apiPostBodyData, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(apiEndpointFullURL, data);
                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return jsonData;
        }

        public KrakenApi()
        {
            using var db = new DatabaseContext();
            var dbMarket = db.Markets.Include(market => market.Credentials).Where(market => market.Name == Name).FirstOrDefault();
            if (dbMarket != null && dbMarket.Credentials.FirstOrDefault() is Credentials dbCredentials)
            {
                credentials = dbCredentials;
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
            var response = await QueryPrivateEndpoint("Balance");

            if (JObject.Parse(response) is not JToken result || result["result"] is not JToken accountBalance)
            {
                throw new Exception("Error during parsing Kraken account balance response.");
            }
            
            if (JsonConvert.DeserializeObject<Dictionary<string, string>>(accountBalance.ToString()) is not Dictionary<string, string> balanceDictionary)
            {
                throw new Exception("Error during deserializing Kraken account balance to dictionary.");
            }

            var balance = new List<Balance>();

            foreach (var kvp in balanceDictionary)
            {
                balance.Add(new Balance
                {
                    Name = kvp.Key,
                    TotalAmount = double.Parse(kvp.Value.ToString(), CultureInfo.InvariantCulture)
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
            var request = new RestRequest("/0/public/OHLC")
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
            var request = new RestRequest("/0/public/Ticker").AddQueryParameter("pair", tradingPair.Name);

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

            var response = await restClient.GetJsonAsync<KrakenResult<KrakenTradingPair>>("/0/public/AssetPairs");
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
