using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Diagrams.Core;

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

        public class StrategyData : INotifyPropertyChanged
        {
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
            public bool Inactive { get => !active; }
            public string StatusText { get => active ? "Active" : "Inactive"; }
            public string StatusButtonContent { get => active ? "Deactivate" : "Activate"; }

            public event PropertyChangedEventHandler? PropertyChanged;

            public StrategyData()
            {
                using (var db = new DatabaseContext())
                strategyCategories = new ObservableCollection<StrategyCategory>(db.StrategyCategories.ToList());
                strategyCategory = strategyCategories.First();
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
            using (var db = new DatabaseContext())
            {
                markets.Clear();
                foreach (var dbMarket in db.Markets.Include(m => m.Credentials).ToList())
                {
                    markets.Add(dbMarket);
                }

                var settingsMarket = await db.GetMarketFromSettings();
                market = markets.Where(m => m.MarketId == settingsMarket.MarketId).First();
                MarketsComboBox.SelectedItem = market;
            }
        }

        private void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
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
                }
            }
        }

        private void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair)
            {
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
                        BuyIndicatorCategory = 1,
                        AskBeforeTrade = false,
                        Active = false
                    });

                    _ = db.SaveChanges();
                }

                if (strategiesQuery.First() is not Strategy dbStrategy)
                {
                    throw new Exception("Couldn't find stored strategy in database.");
                }

                strategyData.Active = dbStrategy.Active;
                strategyData.StrategyCategory = strategyData.StrategyCategories.ElementAt(Math.Abs((int)dbStrategy.BuyIndicatorCategory - 1));
                strategyData.AskBeforeTrade = dbStrategy.AskBeforeTrade;
                strategyData.MaximalLoss = dbStrategy.MaximalLoss.ToString();
                strategyData.MinimalGain = dbStrategy.MinimalGain.ToString();
                strategyData.BuyAmount = dbStrategy.BuyAmount.ToString();
                strategyData.BuyPercentages = dbStrategy.BuyPercentages.ToString();
            }
        }

        private void StrategySwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair)
            {
                using var db = new DatabaseContext();

                var strategiesQuery = db.Strategies.Where(strategy => strategy.TradingPairId == tradingPair.TradingPairId);

                if (strategiesQuery.First() is not Strategy dbStrategy)
                {
                    throw new Exception("Couldn't find stored strategy in database.");
                }

                strategyData.Active = !strategyData.Active;

                dbStrategy.Active = strategyData.Active;
                dbStrategy.BuyIndicatorCategory = (uint)strategyData.StrategyCategories.IndexOf(strategyData.StrategyCategory) + 1;
                dbStrategy.AskBeforeTrade = strategyData.AskBeforeTrade;
                dbStrategy.BuyPercentages = double.Parse(strategyData.BuyPercentages);
                dbStrategy.MaximalLoss = double.Parse(strategyData.MaximalLoss);
                dbStrategy.MinimalGain = double.Parse(strategyData.MinimalGain);
                dbStrategy.BuyAmount = double.Parse(strategyData.BuyAmount);

                _ = db.SaveChanges();

                var tradingPairIds = tradingPairs.Select(tp => tp.TradingPairId).ToArray();
                strategyData.ActiveStrategiesCount = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).Count().ToString();
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
    }
}
