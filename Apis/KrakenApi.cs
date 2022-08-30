using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Services;
using CryptoTA.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CryptoTA.Apis
{
    public class KrakenApi : IMarketApi
    {
        private const string name = "Kraken";
        private readonly uint[] ohlcTimeIntervals = { 60, 300, 900, 1800, 3600, 14400, 86400, 604800, 1296000 };
        private const uint requestMaxTickCount = 720;
        private bool enabled = false;

        private readonly KrakenApiService api;

        public bool Enabled { get => enabled; set => enabled = value; }
        public string Name { get => name; }

        public uint[] OhlcTimeIntervals { get => ohlcTimeIntervals; }

        public uint OhlcMaxDensityTimeInterval { get => ohlcTimeIntervals.Min() * 720; }

        public uint RequestMaxTickCount { get => requestMaxTickCount; }

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

        public class KrakenAsset
        {
            public string? Aclass { get; set; }
            public string? Altname { get; set; }
            public int Decimals { get; set; }
            public int Display_decimals { get; set; }
        }

        public KrakenApi()
        {
            using var db = new DatabaseContext();
            var dbMarket = db.Markets.Include(market => market.Credentials).Where(market => market.Name == Name).FirstOrDefault();
            if (dbMarket != null && dbMarket.Credentials.FirstOrDefault() is Credentials dbCredentials)
            {
                api = new(dbCredentials.PublicKey, dbCredentials.PrivateKey);
            }
            else
            {
                api = new();
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

        public List<Balance> GetAccountBalance()
        {
            var balance = new List<Balance>();

            foreach (var balanceKvp in api.QueryPrivateEndpoint<string>("Balance", new string[] { "result" }))
            {
                balance.Add(new Balance
                {
                    Name = balanceKvp.Key,
                    TotalAmount = double.Parse(balanceKvp.Value.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return balance;
        }

        public async Task<List<Balance>> GetAccountBalanceAsync()
        {
            var balance = new List<Balance>();

            foreach (var balanceKvp in await api.QueryPrivateEndpointAsync<string>("Balance", new string[] { "result" }))
            {
                balance.Add(new Balance
                {
                    Name = balanceKvp.Key,
                    TotalAmount = double.Parse(balanceKvp.Value.ToString(), CultureInfo.InvariantCulture)
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
            var queryParams = new Dictionary<string, string>
            {
                { "pair", tradingPair.Name },
                { "interval", timeInterval.ToString() }
            };

            if (startDate is DateTime date)
            {
                queryParams.Add("since", $"{DateTimeUtils.ToTimestamp(date)}");
            }

            var ohlcTicks = api.QueryPublicEndpoint("OHLC", new string[] { "result", tradingPair.Name }, queryParams);

            var ohlcData = new List<Tick>();

            foreach (JArray krakenOhlcTick in ohlcTicks)
            {
                if (krakenOhlcTick is null)
                {
                    continue;
                }

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
            Dictionary<string, string> queryParams = new() { { "pair", tradingPair.Name } };
            var response = await api.QueryPublicEndpointAsync<KrakenTickerData>("Ticker", new string[] { "result" }, queryParams);
            if (response == null)
            {
                throw new Exception("Error parsing Kraken ticker data");
            }

            var krakenTickerData = response.First().Value;

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

        public async Task<List<Balance>> GetTradingBalance()
        {
            var tradingBalance = new List<Balance>();

            foreach (var tradingBalanceKvp in await api.QueryPrivateEndpointAsync<string>("TradeBalance", new string[] { "result" }))
            {
                tradingBalance.Add(new Balance
                {
                    Name = tradingBalanceKvp.Key,
                    TotalAmount = double.Parse(tradingBalanceKvp.Value.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return tradingBalance;
        }

        public async Task<List<Fees>> GetTradingFees(TradingPair tradingPair)
        {
            var fees = new List<Fees>();

            foreach (var feeKvp in await api.QueryPrivateEndpointAsync<string>("TradeVolume", new string[] { "result", "fees" }, "pair=" + tradingPair.Name))
            {
                fees.Add(new Fees
                {
                    TakerFee = double.Parse(feeKvp.Key.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return fees;
        }

        public async Task<List<TradingPair>> GetTradingPairs()
        {
            var tradingPairs = new List<TradingPair>();

            var response = await api.QueryPublicEndpointAsync<KrakenTradingPair>("AssetPairs", new string[] { "result" });
            if (response == null)
            {
                throw new Exception("Couldn't parse Kraken API trading pairs.");
            }

            foreach (var tradingPairKvp in response)
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

        public async Task<List<Fees>> GetWithdrawalFees(TradingPair tradingPair)
        {
            var fees = new List<Fees>();

            foreach (var feeKvp in await api.QueryPrivateEndpointAsync<string>("TradeVolume", new string[] { "result", "fees" }, "pair=" + tradingPair.Name))
            {
                fees.Add(new Fees
                {
                    TakerFee = double.Parse(feeKvp.Key.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return fees;
        }

        public Task<int> SellOrder(OrderType orderType, double amount, double price)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Trade>> GetTradesHistory()
        {
            var trades = new List<Trade>();

            foreach (var tradeKvp in (await api.QueryPrivateEndpointAsync<Trade>("TradesHistory", new string[] { "result", "trades" })).Skip(1))
            {
                var trade = tradeKvp.Value!;
                trades.Add(new Trade
                {
                    MarketTradeId = tradeKvp.Key,
                    Volume = double.Parse(trade.ToString()!, CultureInfo.InvariantCulture)
                });
            }

            return trades;
        }

        public Asset GetAssetData(string assetName)
        {
            var queryParams = new Dictionary<string, string> { { "assets", assetName } };
            var krakenAsset = api.QueryPublicEndpoint<string>("Assets", new string[] { "result", assetName }, queryParams);

            if (krakenAsset == null)
            {
                throw new Exception("Error during requesting asset info from Kraken API.");
            }

            return new Asset { Name = assetName, Altname = krakenAsset["altname"], Decimals = int.Parse(krakenAsset["decimals"]) };
        }

        public async Task<Asset> GetAssetDataAsync(string assetName)
        {
            var queryParams = new Dictionary<string, string> { { "assets", assetName } };
            var krakenAsset = await api.QueryPublicEndpointAsync<string>("Assets", new string[] { "result", assetName }, queryParams);

            if (krakenAsset == null)
            {
                throw new Exception("Error during requesting asset info from Kraken API.");
            }

            return new Asset { Name = assetName, Altname = krakenAsset["altname"], Decimals = int.Parse(krakenAsset["decimals"]) };
        }
    }
}
