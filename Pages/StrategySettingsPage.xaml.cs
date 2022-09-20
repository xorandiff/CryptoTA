using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Exceptions;
using CryptoTA.UserControls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTA.Pages;

public partial class StrategySettingsPage : Page
{
    private readonly MarketApis marketApis = new();
    private Market market = new();
    private TradingPairStrategy tradingPair = new();
    private readonly StrategyData strategyData;
    private readonly ObservableCollection<Market> markets = new();
    private readonly ObservableCollection<TradingPairStrategy> tradingPairs = new();

    public Market Market
    {
        get => market; set => market = value;
    }
    public TradingPairStrategy TradingPair
    {
        get => tradingPair; set => tradingPair = value;
    }
    public StrategyData Strategy => strategyData;
    public ObservableCollection<Market> Markets => markets;
    public ObservableCollection<TradingPairStrategy> TradingPairs => tradingPairs;

    public class TradingPairStrategy : TradingPair
    {
        public bool HasActiveStrategy { get; set; }

        public TradingPairStrategy()
        {
            HasActiveStrategy = false;
        }

        public TradingPairStrategy(TradingPair tradingPair, bool hasActiveStrategy)
        {
            TradingPairId = tradingPair.TradingPairId;
            Name = tradingPair.Name;
            AlternativeName = tradingPair.AlternativeName;
            WebsocketName = tradingPair.WebsocketName;
            BaseName = tradingPair.BaseName;
            BaseSymbol = tradingPair.BaseSymbol;
            BaseDecimals = tradingPair.BaseDecimals;
            CounterName = tradingPair.CounterName;
            CounterSymbol = tradingPair.CounterSymbol;
            CounterDecimals = tradingPair.CounterDecimals;
            MinimalOrderAmount = tradingPair.MinimalOrderAmount;
            MarketId = tradingPair.MarketId;
            Market = tradingPair.Market;
            Ticks = tradingPair.Ticks;
            HasActiveStrategy = hasActiveStrategy;
        }
    }

    public class StrategyData : INotifyPropertyChanged, IDataErrorInfo
    {
        public Tick currentTick;
        private TradingPairStrategy tradingPair;
        private Asset asset;
        private double minimalGain;
        private double maximalLoss;
        private double buyAmount;
        private double buyPercentages;
        private bool askBeforeTrade;
        private int activeStrategiesCount;
        private bool active;
        private StrategyCategory strategyCategory;
        private readonly ObservableCollection<StrategyCategory> strategyCategories;
        private Visibility hasCredentials;

        public Asset Asset
        {
            get => asset;
            set
            {
                if (value != asset)
                {
                    asset = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Currency));
                    NotifyPropertyChanged(nameof(MinimalOrderAmount));
                }
            }
        }

        public TradingPairStrategy TradingPair
        {
            get => tradingPair;
            set
            {
                if (value != tradingPair)
                {
                    tradingPair = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Currency => asset.AlternativeSymbol;

        public string MinimalOrderAmount
        {
            get
            {
                var baseMinimalAmount = tradingPair.MinimalOrderAmount;

                if (asset.MarketName != tradingPair.BaseSymbol)
                {
                    return (baseMinimalAmount * currentTick.Close).ToString();
                }

                return baseMinimalAmount.ToString();
            }
        }

        public string MinimalGain
        {
            get => minimalGain.ToString();
            set
            {
                var valueDouble = double.Parse(value);
                if (valueDouble != minimalGain)
                {
                    minimalGain = valueDouble;
                    NotifyPropertyChanged();
                }
            }
        }
        public string MaximalLoss
        {
            get => maximalLoss.ToString();
            set
            {
                var valueDouble = double.Parse(value);
                if (valueDouble != maximalLoss)
                {
                    maximalLoss = valueDouble;
                    NotifyPropertyChanged();
                }
            }
        }
        public string BuyAmount
        {
            get => buyAmount.ToString();
            set
            {
                var valueDouble = double.Parse(value);
                if (valueDouble != buyAmount)
                {
                    buyAmount = valueDouble;
                    NotifyPropertyChanged();
                }
            }
        }
        public string BuyPercentages
        {
            get => buyPercentages.ToString();
            set
            {
                var valueDouble = double.Parse(value);
                if (valueDouble != buyPercentages)
                {
                    buyPercentages = valueDouble;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool AskBeforeTrade
        {
            get => askBeforeTrade;
            set
            {
                if (value != askBeforeTrade)
                {
                    askBeforeTrade = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ActiveStrategiesCount
        {
            get => activeStrategiesCount.ToString();
            set
            {
                var valueInt = int.Parse(value);
                if (valueInt != activeStrategiesCount)
                {
                    activeStrategiesCount = valueInt;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(HasActiveStrategies));

                    if (valueInt == 0)
                    {
                        Active = false;
                    }
                }
            }
        }
        public bool HasActiveStrategies => activeStrategiesCount > 0;
        public bool NoFunds
        {
            get => noFunds;
            set
            {
                if (value != noFunds)
                {
                    noFunds = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Inactive));
                    NotifyPropertyChanged(nameof(HasFunds));
                    NotifyPropertyChanged(nameof(StatusText));
                    NotifyPropertyChanged(nameof(StatusButtonContent));
                    NotifyPropertyChanged(nameof(FormVisibility));
                }
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                if (value != active)
                {
                    active = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Inactive));
                    NotifyPropertyChanged(nameof(StatusText));
                    NotifyPropertyChanged(nameof(StatusButtonContent));
                    NotifyPropertyChanged(nameof(FormVisibility));
                }
            }
        }

        public StrategyCategory StrategyCategory
        {
            get => strategyCategory;
            set
            {
                if (value.StrategyCategoryId != strategyCategory.StrategyCategoryId)
                {
                    strategyCategory = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ObservableCollection<StrategyCategory> StrategyCategories => strategyCategories;
        public Visibility FormVisibility
        {
            get => hasCredentials;
            set
            {
                if (value != hasCredentials)
                {
                    hasCredentials = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(WarningVisibility));
                    NotifyPropertyChanged(nameof(HasCredentials));
                }
            }
        }
        public Visibility WarningVisibility 
        {
            get
            {
                if (hasCredentials == Visibility.Visible)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        public bool HasCredentials => hasCredentials == Visibility.Visible;

        public bool noFunds = true;
        public bool Inactive => !active && !noFunds;
        public bool HasFunds => !noFunds;
        public string StatusText => active ? "Active" : "Inactive";
        public string StatusButtonContent => active ? "Deactivate" : "Activate";

        public string Error => "";

        public string this[string name]
        {
            get
            {
                var result = "";

                switch (name)
                {
                    case "MaximalLoss":
                        if (maximalLoss < 0)
                        {
                            result = "Maximal loss cannot be less than zero.";
                        }
                        break;
                    case "BuyAmount":
                        if (buyAmount <= 0)
                        {
                            result = "Buy amount must be positive number.";
                        }
                        break;
                    case "MinimalGain":
                        if (minimalGain < 0)
                        {
                            result = "Minimal gain cannot be less than zero.";
                        }
                        break;
                    case "BuyPercentages":
                        if (buyPercentages <= 0 || buyPercentages > 100)
                        {
                            result = "Buy amount in percentages must be more than 0% and at most 100%.";
                        }
                        break;
                }

                return result;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public StrategyData()
        {
            using var db = new DatabaseContext();
            strategyCategories = new ObservableCollection<StrategyCategory>(db.StrategyCategories.ToList());
            strategyCategory = strategyCategories.First();

            currentTick = new();
            asset = new();
            tradingPair = new();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public StrategySettingsPage()
    {
        InitializeComponent();
        DataContext = this;

        strategyData = new();
    }

    private void CreateTradingPairs()
    {
        tradingPairs.Clear();

        using (var db = new DatabaseContext())
        {
            var activeTradingPairIds = db.Strategies.Where(s => s.Active).Select(s => s.TradingPairId).ToArray();

            var groupedTradingPairs = db.TradingPairs
                                        .Where(tp => tp.MarketId == market.MarketId)
                                        .AsEnumerable()
                                        .GroupBy(tp => activeTradingPairIds.Contains(tp.TradingPairId))
                                        .OrderByDescending(g => g.Key)
                                        .ToList();
            
            foreach (var tradingPairGroup in groupedTradingPairs)
            {
                foreach (var tradingPair in tradingPairGroup)
                {
                    tradingPairs.Add(new TradingPairStrategy(tradingPair, tradingPairGroup.Key));
                }
            }
        }

        TradingPairComboBox.SelectedItem = tradingPairs.First();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        using var db = new DatabaseContext();
        markets.Clear();
        foreach (var dbMarket in db.Markets.Include(m => m.Credentials).ToList())
        {
            markets.Add(dbMarket);
        }

        var settingsMarket = await db.GetMarketFromSettings();
        market = markets.Where(m => m.MarketId == settingsMarket.MarketId).First();
        marketApis.SetActiveApiByName(market.Name);

        MarketsComboBox.SelectedItem = market;
    }

    private bool MarketHasCredentials()
    {
        if (Market is not null && Market.Credentials.Any())
        {
            FeedbackMessageContentControl.Content = null;
            strategyData.FormVisibility = Visibility.Visible;

            return true;
        }
        else
        {
            strategyData.FormVisibility = Visibility.Hidden;
            FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.CredentialsMissing);
            return false;
        }
    }

    private void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MarketHasCredentials())
        {
            marketApis.SetActiveApiByName(market.Name);
            CreateTradingPairs();
        }
    }

    private async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TradingPairComboBox.SelectedItem is TradingPairStrategy)
        {
            FeedbackMessageContentControl.Content = null;
            using var db = new DatabaseContext();

            var strategiesQuery = db.Strategies.Where(strategy => strategy.TradingPairId == tradingPair.TradingPairId);
            var tradingPairIds = tradingPairs.Select(tp => tp.TradingPairId).ToArray();
            strategyData.ActiveStrategiesCount = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).Count().ToString();

            if (!strategiesQuery.Any())
            {
                _ = db.Strategies.Add(new Strategy
                {
                    TradingPairId = tradingPair.TradingPairId,
                    MinimalGain = 1,
                    MaximalLoss = 1,
                    BuyAmount = 0,
                    BuyPercentages = 100,
                    StrategyCategoryId = 1,
                    AskBeforeTrade = false,
                    Active = false
                });

                _ = db.SaveChanges();
            }

            if (await strategiesQuery.FirstAsync() is not Strategy dbStrategy)
            {
                throw new Exception("Couldn't find stored strategy in database.");
            }

            if (await db.StrategyCategories.FindAsync(dbStrategy.StrategyCategoryId) is not StrategyCategory dbStrategyCategory)
            {
                throw new Exception("Couldn't find stored strategy category in database.");
            }

            strategyData.Active = dbStrategy.Active;
            strategyData.StrategyCategory = dbStrategyCategory;
            strategyData.AskBeforeTrade = dbStrategy.AskBeforeTrade;
            strategyData.MaximalLoss = dbStrategy.MaximalLoss.ToString();
            strategyData.MinimalGain = dbStrategy.MinimalGain.ToString();
            strategyData.BuyAmount = dbStrategy.BuyAmount.ToString();
            strategyData.BuyPercentages = dbStrategy.BuyPercentages.ToString();
            strategyData.currentTick = (await marketApis.ActiveMarketApi.GetTickAsync(TradingPair))!;
            strategyData.TradingPair = TradingPair;

            var baseAsset = new Asset { MarketName = TradingPair.BaseSymbol, AlternativeSymbol = TradingPair.BaseName, Decimals = TradingPair.BaseDecimals };
            var counterAsset = new Asset { MarketName = TradingPair.CounterSymbol, AlternativeSymbol = TradingPair.CounterName, Decimals = TradingPair.CounterDecimals };

            strategyData.Asset = counterAsset;

            CurrencyComboBox.ItemsSource = new ObservableCollection<Asset> { baseAsset, counterAsset };
            CurrencyComboBox.SelectedIndex = 0;

            BuyAmountCurrencyComboBox.ItemsSource = new ObservableCollection<TradingPairStrategy> { TradingPair, new TradingPairStrategy { CounterSymbol = "%" } };
            BuyAmountCurrencyComboBox.SelectedIndex = 0;

            var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync();
            var baseVolume = accountBalance.FirstOrDefault(b => b.Name == TradingPair.BaseSymbol)?.TotalAmount ?? 0;
            var counterVolume = accountBalance.FirstOrDefault(b => b.Name == TradingPair.CounterSymbol)?.TotalAmount ?? 0;

            if (baseVolume == 0 && counterVolume == 0)
            {
                FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoFunds);
                strategyData.NoFunds = true;
                return;
            }

            var baseMinimalOrder = tradingPair.MinimalOrderAmount;
            var counterMinimalOrder = Math.Round(baseMinimalOrder * strategyData.currentTick.Close, tradingPair.CounterDecimals);

            if (baseVolume < baseMinimalOrder && counterVolume < counterMinimalOrder)
            {
                var details = $"Minimal order is {baseMinimalOrder} {tradingPair.BaseName} ({counterMinimalOrder} {tradingPair.CounterName})";
                details += $", but your balance is {Math.Round(baseVolume, 5)} {tradingPair.BaseName} ({counterVolume} {tradingPair.CounterName}).";

                FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoMinimalAmount, details);
                strategyData.NoFunds = true;
                return;
            }

            strategyData.NoFunds = false;
            FeedbackMessageContentControl.Content = null;
        }
    }

    private async void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurrencyComboBox.SelectedItem is Asset asset)
        {
            using var db = new DatabaseContext();
            strategyData.currentTick = (await marketApis.ActiveMarketApi.GetTickAsync(TradingPair))!;
            strategyData.Asset = asset;
        }
    }

    private async void StrategySwitchButton_Click(object sender, RoutedEventArgs e)
    {
        if (TradingPairComboBox.SelectedItem is TradingPairStrategy)
        {
            if (strategyData.Inactive)
            {
                StrategySwitchButton.IsEnabled = false;

                try
                {
                    var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync();
                    var baseVolume = accountBalance.FirstOrDefault(b => b.Name == TradingPair.BaseSymbol)?.TotalAmount ?? 0;
                    var counterVolume = accountBalance.FirstOrDefault(b => b.Name == TradingPair.CounterSymbol)?.TotalAmount ?? 0;

                    strategyData.currentTick = (await marketApis.ActiveMarketApi.GetTickAsync(TradingPair))!;
                    var baseMinimalOrder = TradingPair.MinimalOrderAmount;
                    var counterMinimalOrder = Math.Round(baseMinimalOrder * strategyData.currentTick.Close, tradingPair.CounterDecimals);

                    if (baseVolume == 0 && counterVolume == 0)
                    {
                        FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoFunds);
                        return;
                    }
                    else if (baseVolume < baseMinimalOrder && counterVolume < counterMinimalOrder)
                    {
                        var details = $"Minimal order is {baseMinimalOrder} {tradingPair.BaseName} ({counterMinimalOrder} {tradingPair.CounterName})";
                        details += $", but your balance is {Math.Round(baseVolume, 5)} {tradingPair.BaseName} ({counterVolume} {tradingPair.CounterName}).";

                        FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoMinimalAmount, details);
                        return;
                    }
                    else
                    {
                        FeedbackMessageContentControl.Content = null;
                    }
                }
                catch (KrakenApiException krakenApiException)
                {
                    FeedbackMessageContentControl.Content = new FeedbackMessage(krakenApiException.Message);
                }
                finally
                {
                    StrategySwitchButton.IsEnabled = true;
                }

                StrategySwitchButton.IsEnabled = true;
            }

            strategyData.Active = !strategyData.Active;
            TradingPair.HasActiveStrategy = strategyData.Active;

            SaveChanges();
        }
    }

    private void StrategiesSwitchButton_Click(object sender, RoutedEventArgs e)
    {
        if (TradingPairComboBox.SelectedItem is TradingPairStrategy)
        {
            using var db = new DatabaseContext();

            var tradingPairIds = tradingPairs.Select(tp => tp.TradingPairId).ToArray();
            var dbStrategies = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).ToArray();

            foreach (var dbStrategy in dbStrategies)
            {
                dbStrategy.Active = false;
            }

            _ = db.SaveChanges();

            strategyData.ActiveStrategiesCount = "0";
        }
    }

    private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
    {
        SaveChanges();
    }

    private void SaveChanges()
    {
        if (TradingPairComboBox.SelectedItem is TradingPairStrategy && BuyAmountCurrencyComboBox.SelectedItem is TradingPairStrategy currencyTradingPair)
        {
            using var db = new DatabaseContext();

            var strategiesQuery = db.Strategies.Where(strategy => strategy.TradingPairId == tradingPair.TradingPairId);

            if (strategiesQuery.First() is not Strategy dbStrategy)
            {
                throw new Exception("Couldn't find stored strategy in database.");
            }

            if (db.StrategyCategories.Find(dbStrategy.StrategyCategoryId) is not StrategyCategory dbStrategyCategory)
            {
                throw new Exception("Couldn't find stored strategy category in database.");
            }

            dbStrategy.Active = strategyData.Active;
            dbStrategy.StrategyCategoryId = dbStrategyCategory.StrategyCategoryId;
            dbStrategy.AskBeforeTrade = strategyData.AskBeforeTrade;
            dbStrategy.BuyPercentages = currencyTradingPair.CounterSymbol == "%" ? double.Parse(strategyData.BuyAmount) : 0d;
            dbStrategy.MaximalLoss = double.Parse(strategyData.MaximalLoss);
            dbStrategy.MinimalGain = double.Parse(strategyData.MinimalGain);
            dbStrategy.BuyAmount = currencyTradingPair.CounterSymbol != "%" ? double.Parse(strategyData.BuyAmount) : 0d;

            _ = db.SaveChanges();

            var tradingPairIds = tradingPairs.Select(tp => tp.TradingPairId).ToArray();
            strategyData.ActiveStrategiesCount = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).Count().ToString();
        }
    }
}
