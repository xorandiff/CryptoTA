using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTA.Pages
{
    public partial class StrategySettingsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private Market market = new();
        private TradingPair tradingPair = new();
        private ObservableCollection<Market> markets = new();
        private ObservableCollection<TradingPair> tradingPairs = new();
        private bool strategyEnabled = false;
        private uint activeStrategiesCount = 0;

        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public ObservableCollection<Market> Markets { get => markets; }
        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }
        public bool StrategyEnabled { get => strategyEnabled; }
        public uint ActiveStrategiesCount { get => activeStrategiesCount; }
        public bool HasActiveStrategy { get => activeStrategiesCount > 0; }


        public StrategySettingsPage()
        {
            InitializeComponent();
            DataContext = this;
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
                TradingPair = await db.GetTradingPairFromSettings();
                TradingPair = tradingPairs.Where(tp => tp.TradingPairId == TradingPair.TradingPairId).First();
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
            if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
            {

            }
        }
    }
}
