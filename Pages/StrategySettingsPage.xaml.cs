using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace CryptoTA.Pages
{
    public partial class StrategySettingsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private Market market = new();
        private TradingPair tradingPair = new();
        private Strategy strategy = new();
        private StrategyCategory strategyCategory = new();
        private ObservableCollection<Market> markets = new();
        private ObservableCollection<TradingPair> tradingPairs = new();
        private ObservableCollection<StrategyCategory> strategyCategories;
        private bool strategyEnabled = false;

        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public Strategy Strategy { get => strategy; set => strategy = value; }
        public StrategyCategory StrategyCategory { get => strategyCategory; set => strategyCategory = value; }
        public ObservableCollection<Market> Markets { get => markets; }
        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }
        public ObservableCollection<StrategyCategory> StrategyCategories { get => strategyCategories; }
        public bool StrategyEnabled { get => strategyEnabled; }

        public StrategySettingsPage()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new DatabaseContext())
            strategyCategories = new ObservableCollection<StrategyCategory>(db.StrategyCategories.ToList());
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

                tradingPairs.Clear();
                foreach (var tradingPair in CreateTradingPairs())
                {
                    tradingPairs.Add(tradingPair);
                }
            }
        }

        private void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
                    CredentialsMissingGroupBox.Visibility = Visibility.Hidden;
                    StrategyGroupBox.IsEnabled = true;
                    StrategyStatusGroupBox.IsEnabled = true;
                    StrategiesStatusGroupBox.IsEnabled = true;

                    if (!marketApis.setActiveApiByName(Market.Name))
                    {
                        throw new Exception("No market API found that correspond to database market name.");
                    }

                    // ...
                }
                else
                {
                    CredentialsMissingGroupBox.Visibility = Visibility.Visible;
                    StrategyGroupBox.IsEnabled = false;
                    StrategyStatusGroupBox.IsEnabled = false;
                    StrategiesStatusGroupBox.IsEnabled = false;
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
                ActiveStrategiesCountTextBlock.Text = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).Count().ToString();
                StrategiesSwitchButton.IsEnabled = ActiveStrategiesCountTextBlock.Text != "0";

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

                strategy = dbStrategy;

                StrategySwitchButton.IsEnabled = true;
                StrategySwitchButton.Content = strategy.Active ? "Deactivate" : "Activate";
                StrategyStatusTextBlock.Text = strategy.Active ? "Active" : "Inactive";
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

                strategy.Active = !strategy.Active;
                dbStrategy = strategy;

                StrategyStatusTextBlock.Text = dbStrategy.Active ? "Inactive" : "Active";
                StrategySwitchButton.Content = dbStrategy.Active ? "Activate" : "Deactivate";

                _ = db.SaveChanges();

                var tradingPairIds = tradingPairs.Select(tp => tp.TradingPairId).ToArray();
                ActiveStrategiesCountTextBlock.Text = db.Strategies.Where(s => s.Active && tradingPairIds.Contains(s.TradingPairId)).Count().ToString();
                StrategiesSwitchButton.IsEnabled = ActiveStrategiesCountTextBlock.Text != "0";
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

                StrategiesSwitchButton.IsEnabled = false;
                ActiveStrategiesCountTextBlock.Text = "0";
                StrategyStatusTextBlock.Text = "Inactive";
                StrategySwitchButton.Content = "Activate";
            }
        }
    }
}
