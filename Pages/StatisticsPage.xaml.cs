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
    public partial class StatisticsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private Market market = new();
        private ObservableCollection<Market> markets = new();

        public ObservableCollection<Market> Markets { get => markets; }
        public Market Market { get => market; set => market = value; }

        public StatisticsPage()
        {
            InitializeComponent();
            DataContext = this;
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

        private async void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
                    MarketsComboBox.IsEnabled = false;

                    WarningGroupBox.Visibility = Visibility.Hidden;
                    if (!marketApis.setActiveApiByName(Market.Name))
                    {
                        throw new Exception("No market API found that correspond to database market name.");
                    }

                    try
                    {
                        var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync();
                        var tradingBalance = await marketApis.ActiveMarketApi.GetTradingBalance();

                        AccountBalanceItemsControl.ItemsSource = accountBalance.Concat(tradingBalance);
                    }
                    catch (Exception)
                    {
                        WarningGroupBox.Visibility = Visibility.Visible;
                        WarningHeader.Text = "Invalid Credentials";
                        WarningText1.Text = "Server rejected your credentials.";
                        WarningText2.Text = "You can change them by choosing Edit -> Accounts from menu.";
                    }
                    finally
                    {
                        MarketsComboBox.IsEnabled = true;
                    }
                }
                else
                {
                    WarningGroupBox.Visibility = Visibility.Visible;
                    WarningHeader.Text = "Credentials Missing";
                    WarningText1.Text = "You have to provide credentials for this API.";
                    WarningText2.Text = "You can add them by choosing Edit -> Accounts from menu.";
                }
            }
        }
    }
}
