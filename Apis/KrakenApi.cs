using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Exceptions;
using CryptoTA.Services;
using CryptoTA.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoTA.Apis
{
    using UriParams = Dictionary<string, string>;

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

        public class KrakenTradeData
        {
            public string Ordertxid { get; set; }
            public string Pair { get; set; }
            public long Time { get; set; }
            public string Type { get; set; }
            public string Ordertype { get; set; }
            public string Price { get; set; }
            public string Cost { get; set; }
            public string Fee { get; set; }
            public string Vol { get; set; }
            public string Margin { get; set; }
            public string Misc { get; set; }
            public string? Posstatus { get; set; }
            public string? Cprice { get; set; }
            public string? Ccost { get; set; }
            public string? Cfee { get; set; }
            public string? Cvol { get; set; }
            public string? Cmargin { get; set; }
            public string? Net { get; set; }
            public string[]? Trades { get; set; }
        }

        public class KrakenOrderDescription
        {
            public string Pair { get; set; }
            public string Type { get; set; }
            public string Ordertype { get; set; }
            public string Price { get; set; }
            public string Price2 { get; set; }
            public string Leverage { get; set; }
            public string Order { get; set; }
            public string Close { get; set; }
        }

        public class KrakenOrderData
        {
            public string Refid { get; set; }
            public string Userref { get; set; }
            public string Status { get; set; }
            public long Opentm { get; set; }
            public long Starttm { get; set; }
            public long Expiretm { get; set; }
            public KrakenOrderDescription Descr { get; set; }
            public string Vol { get; set; }
            public string Vol_exec { get; set; }
            public string Cost { get; set; }
            public string Fee { get; set; }
            public string Price { get; set; }
            public string Stopprice { get; set; }
            public string Limitprice { get; set; }
            public string Trigger { get; set; }
            public string Misc { get; set; }
            public string Oflags { get; set; }
            public string[]? Trades { get; set; }
        }

        public class KrakenLedger
        {
            public string Refid { get; set; }
            public long Time { get; set; }
            public string Type { get; set; }
            public string Subtype { get; set; }
            public string Aclass { get; set; }
            public string Asset { get; set; }
            public string Amount { get; set; }
            public string Fee { get; set; }
            public string Balance { get; set; }
        }

        public class KrakenOrderResultDescription
        {
            public string Order { get; set; }
            public string Close { get; set; }
        }

        public class KrakenOrderResult
        {
            public KrakenOrderResultDescription Descr { get; set; }
            public string[] Txid { get; set; }
        }

        public class KrakenFeesData
        {
            public string Fee { get; set; }
            public string Min_fee { get; set; }
            public string Max_fee { get; set; }
            public string Next_fee { get; set; }
            public string Tier_volume { get; set; }
            public string Next_volume { get; set; }
        }

        public class KrakenTradingFeesResult
        {
            public string Currency { get; set; }
            public string Volume { get; set; }
            public Dictionary<string, KrakenFeesData> Fees { get; set; }
            public Dictionary<string, KrakenFeesData> Fees_maker { get; set; }
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

        public async Task<bool> CancelAllOrders()
        {
            var resultCount = await api.QueryPrivateEndpointAsync("CancelAll", new string[] { "result", "count" });

            if (int.TryParse(resultCount.ToString(), out int count))
            {
                return count > 0;
            }

            return false;
        }

        public async Task<bool> CancelOrder(int transactionIdOrUserRef)
        {
            var resultCount = await api.QueryPrivateEndpointAsync("CancelOrder", new string[] { "result", "count" }, new UriParams { { "txid", transactionIdOrUserRef.ToString() } });

            if (int.TryParse(resultCount.ToString(), out int count))
            {
                return count > 0;
            }

            return false;
        }

        public List<Balance> GetAccountBalance()
        {
            var accountBalance = new List<Balance>();

            foreach (var balanceKvp in api.QueryPrivateEndpoint<string>("Balance", new string[] { "result" }))
            {
                accountBalance.Add(new Balance
                {
                    Name = balanceKvp.Key,
                    TotalAmount = double.Parse(balanceKvp.Value.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return accountBalance;
        }

        public List<Balance> GetAccountBalance(TradingPair tradingPair)
        {
            string? baseAltName = null;
            string? counterAltName = null;

            if (tradingPair.DisplayName is not null && tradingPair.DisplayName.Contains("/"))
            {
                var pairNames = tradingPair.DisplayName.Split("/");
                baseAltName = pairNames[0];
                counterAltName = pairNames[1];
            }

            var accountBalance = new List<Balance>
            {
                new() { Name = tradingPair.BaseName, TotalAmount = 0 },
                new() { Name = tradingPair.CounterName, TotalAmount = 0 }
            };

            List<Balance> apiAccountBalance = GetAccountBalance() ?? new();

            if (accountBalance.Count == 0)
            {
                return accountBalance;
            }

            var matchedBalances = apiAccountBalance
                                    .Where(b => !string.IsNullOrWhiteSpace(b.Name) && tradingPair.Name.Contains(b.Name))
                                    .ToList(); 
            
            foreach (var matchedBalance in matchedBalances)
            {
                if ((baseAltName is not null && baseAltName == matchedBalance.Name) || tradingPair.Name.StartsWith(matchedBalance.Name!))
                {
                    accountBalance[0].TotalAmount = matchedBalance.TotalAmount;
                }
                else if ((counterAltName is not null && counterAltName == matchedBalance.Name) || tradingPair.Name.EndsWith(matchedBalance.Name!))
                {
                    accountBalance[1].TotalAmount = matchedBalance.TotalAmount;
                }
            }

            return accountBalance;
        }

        public async Task<List<Balance>> GetAccountBalanceAsync(TradingPair tradingPair)
        {
            string? baseAltName = null;
            string? counterAltName = null;

            if (tradingPair.DisplayName is not null && tradingPair.DisplayName.Contains("/"))
            {
                var pairNames = tradingPair.DisplayName.Split("/");
                baseAltName = pairNames[0];
                counterAltName = pairNames[1];
            }

            var accountBalance = new List<Balance>
            {
                new() { Name = tradingPair.BaseName, TotalAmount = 0 },
                new() { Name = tradingPair.CounterName, TotalAmount = 0 }
            };

            List<Balance> apiAccountBalance = await GetAccountBalanceAsync() ?? new();

            if (accountBalance.Count == 0)
            {
                return accountBalance;
            }

            var matchedBalances = apiAccountBalance
                                    .Where(b => !string.IsNullOrWhiteSpace(b.Name) && tradingPair.Name.Contains(b.Name))
                                    .ToList();

            foreach (var matchedBalance in matchedBalances)
            {
                if ((baseAltName is not null && baseAltName == matchedBalance.Name) || tradingPair.Name.StartsWith(matchedBalance.Name!))
                {
                    accountBalance[0].TotalAmount = matchedBalance.TotalAmount;
                }
                else if ((counterAltName is not null && counterAltName == matchedBalance.Name) || tradingPair.Name.EndsWith(matchedBalance.Name!))
                {
                    accountBalance[1].TotalAmount = matchedBalance.TotalAmount;
                }
            }

            return accountBalance;
        }

        public async Task<List<Balance>> GetAccountBalanceAsync()
        {
            var accountBalance = new List<Balance>();

            foreach (var balanceKvp in await api.QueryPrivateEndpointAsync<string>("Balance", new string[] { "result" }))
            {
                accountBalance.Add(new Balance
                {
                    Name = balanceKvp.Key,
                    TotalAmount = double.Parse(balanceKvp.Value.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return accountBalance;
        }

        public async Task<List<Order>> GetClosedOrders()
        {
            var closedOrders = new List<Order>();

            foreach (var orderKvp in await api.QueryPrivateEndpointAsync<KrakenOrderData>("ClosedOrders", new string[] { "result", "closed" }))
            {
                if (orderKvp.Value is not KrakenOrderData orderData)
                {
                    continue;
                }

                using var db = new DatabaseContext();
                var dbTradingPair = db.TradingPairs.Where(tp => tp.Name == orderData.Descr.Pair).First();

                if (dbTradingPair is not TradingPair tradingPair)
                {
                    continue;
                }

                closedOrders.Add(new Order
                {
                    MarketOrderId = orderData.Refid,
                    OrderType = orderData.Descr.Ordertype,
                    Status = orderData.Status,
                    TotalCost = double.Parse(orderData.Cost, CultureInfo.InvariantCulture),
                    AveragePrice = double.Parse(orderData.Price, CultureInfo.InvariantCulture),
                    Fee = double.Parse(orderData.Fee, CultureInfo.InvariantCulture),
                    Volume = double.Parse(orderData.Vol, CultureInfo.InvariantCulture),
                    VolumeExecuted = double.Parse(orderData.Vol_exec, CultureInfo.InvariantCulture),
                    StartDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                    OpenDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                    ExpireDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                    TradingPairId = tradingPair.TradingPairId
                });
            }

            return closedOrders;
        }

        public async Task GetOhlcDataWsAsync(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            var payload = new
            {
                @event = "subscribe",
                pair = new string[] { "XBT/USD" },
                subscription = new { name = "ohlc" }
            };

            await api.QueryWebsocketApi(payload);
        }

        public List<Tick> GetOhlcData(TradingPair tradingPair, DateTime? startDate, uint timeInterval)
        {
            var queryParams = new UriParams
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

        public async Task<List<Order>> GetOpenOrders()
        {
            var openOrders = new List<Order>();

            foreach (var orderKvp in await api.QueryPrivateEndpointAsync<KrakenOrderData>("OpenOrders", new string[] { "result", "open" }))
            {
                if (orderKvp.Value is not KrakenOrderData orderData)
                {
                    continue;
                }

                using (var db = new DatabaseContext())
                {
                    var dbTradingPair = db.TradingPairs.Where(tp => tp.Name == orderData.Descr.Pair).First();

                    if (dbTradingPair is not TradingPair tradingPair)
                    {
                        continue;
                    }

                    openOrders.Add(new Order
                    {
                        MarketOrderId = orderData.Refid,
                        OrderType = orderData.Descr.Ordertype,
                        Status = orderData.Status,
                        TotalCost = double.Parse(orderData.Cost, CultureInfo.InvariantCulture),
                        AveragePrice = double.Parse(orderData.Price, CultureInfo.InvariantCulture),
                        Fee = double.Parse(orderData.Fee, CultureInfo.InvariantCulture),
                        Volume = double.Parse(orderData.Vol, CultureInfo.InvariantCulture),
                        VolumeExecuted = double.Parse(orderData.Vol_exec, CultureInfo.InvariantCulture),
                        StartDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                        OpenDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                        ExpireDate = DateTimeUtils.FromTimestamp(orderData.Starttm),
                        TradingPairId = tradingPair.TradingPairId
                    });
                }
            }

            return openOrders;
        }

        public async Task<OrderBook> GetOrderBook(TradingPair tradingPair)
        {
            UriParams queryParams = new() { { "pair", tradingPair.Name } };

            var response = await api.QueryPublicEndpointAsync<JArray>("Depth", new string[] { "result", tradingPair.Name }, queryParams);

            if (response is null || response["asks"] is not JArray asks || response["bid"] is not JArray bids)
            {
                throw new Exception("Error parsing Kraken ticker data");
            }

            var orderBook = new OrderBook
            {
                Asks = new List<OrderBookEntry>(),
                Bids = new List<OrderBookEntry>()
            };

            foreach (var ask in asks)
            {
                orderBook.Asks.Add(new OrderBookEntry
                {
                    Price = double.Parse(ask[0]!.ToString(), CultureInfo.InvariantCulture),
                    Volume = double.Parse(ask[1]!.ToString(), CultureInfo.InvariantCulture),
                    Date = DateTimeUtils.FromTimestamp((long)ask[2]!)
                });
            }

            foreach (var bid in bids)
            {
                orderBook.Bids.Add(new OrderBookEntry
                {
                    Price = double.Parse(bid[0]!.ToString(), CultureInfo.InvariantCulture),
                    Volume = double.Parse(bid[1]!.ToString(), CultureInfo.InvariantCulture),
                    Date = DateTimeUtils.FromTimestamp((long)bid[2]!)
                });
            }

            return orderBook;
        }

        public Tick? GetTick(TradingPair tradingPair)
        {
            UriParams queryParams = new() { { "pair", tradingPair.Name } };

            var response = api.QueryPublicEndpoint<KrakenTickerData>("Ticker", new string[] { "result" }, queryParams);

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

        public List<Fees> GetTradingFees(TradingPair tradingPair)
        {
            var fees = new List<Fees>();

            UriParams bodyParams = new() { { "pair", tradingPair.Name } };

            var jResult = api.QueryPrivateEndpoint("TradeVolume", new string[] { "result" }, bodyParams);

            if (JsonConvert.DeserializeObject<KrakenTradingFeesResult>(jResult.ToString()) is not KrakenTradingFeesResult krakenTradingFeesResult)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            fees.Add(new Fees
            {
                TakerFee = double.Parse(krakenTradingFeesResult.Fees.First().Value.Fee, CultureInfo.InvariantCulture),
                MakerFee = double.Parse(krakenTradingFeesResult.Fees_maker.First().Value.Fee, CultureInfo.InvariantCulture),
            });

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

        public async Task<WebsocketsToken> GetWebsocketsToken()
        {
            var result = await api.QueryPrivateEndpointAsync<JValue>("GetWebSocketsToken", new string[] { "result" });

            if (result is null || result["token"] is null || result["expires"] is null)
            {
                throw new Exception("Error during processing response from Kraken API.");
            }

            return new WebsocketsToken
            {
                Token = result["token"].ToString(),
                ExpirationDate = DateTimeUtils.FromTimestamp((long)result["expires"])
            };
        }

        public async Task<List<Fees>> GetWithdrawalFees(TradingPair tradingPair)
        {
            var fees = new List<Fees>();

            UriParams bodyParams = new() { { "pair", tradingPair.Name } };

            foreach (var feeKvp in await api.QueryPrivateEndpointAsync<string>("TradeVolume", new string[] { "result", "fees" }, bodyParams))
            {
                fees.Add(new Fees
                {
                    TakerFee = double.Parse(feeKvp.Key.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return fees;
        }

        public async Task<List<Trade>> GetTradesHistory()
        {
            var trades = new List<Trade>();

            var tradeHistory = await api.QueryPrivateEndpointAsync<KrakenTradeData>("TradesHistory", new string[] { "result", "trades" });

            foreach (var tradeKvp in tradeHistory)
            {
                if (tradeKvp.Value is not KrakenTradeData tradeData)
                {
                    continue;
                }

                using var db = new DatabaseContext();
                var dbTradingPair = db.TradingPairs.Where(tp => tp.Name == tradeData.Pair).First();

                if (dbTradingPair is not TradingPair tradingPair)
                {
                    continue;
                }

                trades.Add(new Trade
                {
                    MarketTradeId = tradeKvp.Key,
                    MarketOrderId = tradeData.Ordertxid,
                    OrderType = tradeData.Ordertype,
                    Type = tradeData.Type,
                    Cost = double.Parse(tradeData.Cost, CultureInfo.InvariantCulture),
                    Price = double.Parse(tradeData.Price, CultureInfo.InvariantCulture),
                    Fee = double.Parse(tradeData.Fee, CultureInfo.InvariantCulture),
                    Volume = double.Parse(tradeData.Vol, CultureInfo.InvariantCulture),
                    Date = DateTimeUtils.FromTimestamp(tradeData.Time),
                    TradingPairId = tradingPair.TradingPairId
                });
            }

            return trades;
        }

        public Asset GetAssetData(string assetName)
        {
            var queryParams = new UriParams { { "assets", assetName } };
            var krakenAsset = api.QueryPublicEndpoint<string>("Assets", new string[] { "result", assetName }, queryParams);

            if (krakenAsset == null)
            {
                throw new Exception("Error during requesting asset info from Kraken API.");
            }

            return new Asset { Name = assetName, Altname = krakenAsset["altname"], Decimals = int.Parse(krakenAsset["decimals"]) };
        }

        public async Task<Asset> GetAssetDataAsync(string assetName)
        {
            var queryParams = new UriParams { { "assets", assetName } };
            var krakenAsset = await api.QueryPublicEndpointAsync<string>("Assets", new string[] { "result", assetName }, queryParams);

            if (krakenAsset == null)
            {
                throw new Exception("Error during requesting asset info from Kraken API.");
            }

            return new Asset { Name = assetName, Altname = krakenAsset["altname"], Decimals = int.Parse(krakenAsset["decimals"]) };
        }

        public async Task<List<Ledger>> GetLedgers()
        {
            var ledgers = new List<Ledger>();

            foreach (var ledgerKvp in await api.QueryPrivateEndpointAsync<KrakenLedger>("Ledgers", new string[] { "result", "ledger" }))
            {
                if (ledgerKvp.Value is not KrakenLedger krakenLedger)
                {
                    continue;
                }

                ledgers.Add(new Ledger
                {
                    MarketLedgerId = ledgerKvp.Key,
                    ReferenceId = krakenLedger.Refid,
                    Type = krakenLedger.Type,
                    Subtype = krakenLedger.Subtype,
                    AssetClass = krakenLedger.Aclass,
                    Asset = krakenLedger.Asset,
                    Date = DateTimeUtils.FromTimestamp(krakenLedger.Time),
                    Amount = double.Parse(krakenLedger.Amount, CultureInfo.InvariantCulture),
                    Fee = double.Parse(krakenLedger.Fee, CultureInfo.InvariantCulture),
                    Balance = double.Parse(krakenLedger.Balance, CultureInfo.InvariantCulture)
                });
            }

            return ledgers;
        }

        public async Task<Tick?> GetTickAsync(TradingPair tradingPair)
        {
            UriParams queryParams = new() { { "pair", tradingPair.Name } };

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

        public async Task<List<Fees>> GetTradingFeesAsync(TradingPair tradingPair)
        {
            var fees = new List<Fees>();

            UriParams bodyParams = new() { { "pair", tradingPair.Name } };

            foreach (var feeKvp in await api.QueryPrivateEndpointAsync<string>("TradeVolume", new string[] { "result", "fees" }, bodyParams))
            {
                fees.Add(new Fees
                {
                    TakerFee = double.Parse(feeKvp.Key.ToString(), CultureInfo.InvariantCulture)
                });
            }

            return fees;
        }

        public string BuyOrder(TradingPair tradingPair, OrderType orderType, double amount)
        {
            var bodyParams = new UriParams
            {
                { "ordertype", orderType.ToString() },
                { "type", "buy" },
                { "volume", amount.ToString() },
                { "pair", tradingPair.Name },
            };

            var krakenResult = api.QueryPrivateEndpoint<KrakenOrderResult>("AddOrder", null, bodyParams);

            if (krakenResult is null || krakenResult.First().Value is not KrakenOrderResult krakenOrderResult)
            {
                return "";
            }

            return krakenOrderResult.Txid.First();
        }

        public async Task<string> BuyOrderAsync(TradingPair tradingPair, OrderType orderType, double amount)
        {
            var bodyParams = new UriParams
            {
                { "ordertype", orderType.ToString() },
                { "type", "buy" },
                { "volume", amount.ToString() },
                { "pair", tradingPair.Name },
            };

            var krakenResult = await api.QueryPrivateEndpointAsync<KrakenOrderResult>("AddOrder", null, bodyParams);

            if (krakenResult is null || krakenResult.First().Value is not KrakenOrderResult krakenOrderResult)
            {
                return "";
            }

            return krakenOrderResult.Txid.First();
        }

        public string SellOrder(TradingPair tradingPair, OrderType orderType, double amount)
        {
            var bodyParams = new UriParams
            {
                { "ordertype", orderType.ToString() },
                { "type", "sell" },
                { "volume", amount.ToString() },
                { "pair", tradingPair.Name },
            };

            var krakenResult = api.QueryPrivateEndpoint<KrakenOrderResult>("AddOrder", null, bodyParams);

            if (krakenResult is null || krakenResult.First().Value is not KrakenOrderResult krakenOrderResult)
            {
                return "";
            }

            return krakenOrderResult.Txid.First();
        }

        public async Task<string> SellOrderAsync(TradingPair tradingPair, OrderType orderType, double amount)
        {
            var bodyParams = new UriParams
            {
                { "ordertype", orderType.ToString() },
                { "type", "sell" },
                { "volume", amount.ToString() },
                { "pair", tradingPair.Name },
            };

            var krakenResult = await api.QueryPrivateEndpointAsync<KrakenOrderResult>("AddOrder", null, bodyParams);

            if (krakenResult is null || krakenResult.First().Value is not KrakenOrderResult krakenOrderResult)
            {
                return "";
            }

            return krakenOrderResult.Txid.First();
        }
    }
}
