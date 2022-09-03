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

namespace CryptoTA.Pages
{
    public partial class StrategySettingsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private Market market = new();
        private TradingPair tradingPair = new();
        private StrategyData strategyData;
        private ObservableCollection<Market> markets = new();
        private ObservableCollection<TradingPair> tradingPairs = new();

        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public StrategyData Strategy { get => strategyData; }
        public ObservableCollection<Market> Markets { get => markets; }
        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }

        public class StrategyData : INotifyPropertyChanged, IDataErrorInfo
        {
            private TradingPair tradingPair;
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

            public TradingPair TradingPair
            {
                get => tradingPair;
                set
                {
                    if (value != tradingPair)
                    {
                        tradingPair = value;
                        NotifyPropertyChanged();
                        NotifyPropertyChanged(nameof(Currency));
                    }
                }
            }

            public string Currency
            {
                get => tradingPair.CounterSymbol;
            }

            public string MinimalGain
            {
                get => minimalGain.ToString();
                set
                {
                    double valueDouble = double.Parse(value);
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
                    double valueDouble = double.Parse(value);
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
                    double valueDouble = double.Parse(value);
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
                    double valueDouble = double.Parse(value);
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
                    int valueInt = int.Parse(value);
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
            public bool HasActiveStrategies { get => activeStrategiesCount > 0; }
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
            public ObservableCollection<StrategyCategory> StrategyCategories { get => strategyCategories; }
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
            public bool noFunds = true;
            public bool Inactive => !active && !noFunds;
            public bool HasFunds => !noFunds;
            public string StatusText { get => active ? "Active" : "Inactive"; }
            public string StatusButtonContent { get => active ? "Deactivate" : "Activate"; }

            public string Error => "";

            public string this[string name]
            {
                get
                {
                    string result = "";

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
                using (var db = new DatabaseContext())
                strategyCategories = new ObservableCollection<StrategyCategory>(db.StrategyCategories.ToList());
                strategyCategory = strategyCategories.First();

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

        private ObservableCollection<TradingPair> CreateTradingPairs()
        {
            ObservableCollection<TradingPair> tradingPairs = new();

            using (var db = new DatabaseContext())
            {
                foreach (TradingPair tradingPair in db.TradingPairs.Where(tp => tp.MarketId == market.MarketId).ToList())
                {
                    tradingPairs.Add(tradingPair);
                }
            }

            return tradingPairs;
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
            MarketsComboBox.SelectedItem = market;
        }

        private void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
                    FeedbackMessageContentControl.Content = null;
                    strategyData.FormVisibility = Visibility.Visible;
                    if (!marketApis.setActiveApiByName(Market.Name))
                    {
                        throw new Exception("No market API found that correspond to database market name.");
                    }

                    tradingPairs.Clear();
                    foreach (var tradingPair in CreateTradingPairs())
                    {
                        tradingPairs.Add(tradingPair);
                    }
                    TradingPairComboBox.SelectedItem = tradingPairs.First();
                }
                else
                {
                    strategyData.FormVisibility = Visibility.Hidden;
                    FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.CredentialsMissing);
                }
            }
        }

        private async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair)
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
                strategyData.TradingPair = TradingPair;

                CurrencyComboBox.ItemsSource = new ObservableCollection<TradingPair> { TradingPair };
                CurrencyComboBox.SelectedIndex = 0;

                BuyAmountCurrencyComboBox.ItemsSource = new ObservableCollection<TradingPair> { TradingPair, new TradingPair { CounterSymbol = "%" } };
                BuyAmountCurrencyComboBox.SelectedIndex = 0;

                var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync(tradingPair);
                if (accountBalance is null || !accountBalance.Any(b => b.TotalAmount > 0d))
                {
                    FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoFunds);
                    strategyData.NoFunds = true;
                    return;
                }

                strategyData.NoFunds = false;
                FeedbackMessageContentControl.Content = null;
            }
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void StrategySwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair)
            {
                if (strategyData.Inactive)
                {
                    StrategySwitchButton.IsEnabled = false;

                    try
                    {
                        var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync(tradingPair);
                        if (accountBalance is null || !accountBalance.Any(b => b.TotalAmount > 0d))
                        {
                            FeedbackMessageContentControl.Content = new FeedbackMessage(MessageType.StrategyNoFunds);
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
                SaveChanges();
            }
        }

        private void StrategiesSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair)
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
            if (TradingPairComboBox.SelectedItem is TradingPair && BuyAmountCurrencyComboBox.SelectedItem is TradingPair currencyTradingPair)
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
}
