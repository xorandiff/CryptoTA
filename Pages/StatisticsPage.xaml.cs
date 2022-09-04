using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Exceptions;
using CryptoTA.UserControls;
using CryptoTA.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CryptoTA.Pages
{
    [ValueConversion(typeof(string), typeof(bool?))]
    public class SignConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string currency = (string)value;
            if (currency.Split(" ")[0] == "0")
            {
                return null;
            }
            return !currency.StartsWith("-");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class LedgerDisplay
    {
        public string MarketLedgerId { get; set; }
        public string ReferenceId { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string AssetClass { get; set; }
        public string Asset { get; set; }
        public string Amount { get; set; }
        public string Fee { get; set; }
        public string Balance { get; set; }
    }

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
            if (Market is null || !Market.Credentials.Any())
            {
                MessageBoxGrid.Content = new FeedbackMessage(MessageType.CredentialsMissing);
                return;
            }

            MarketsComboBox.IsEnabled = false;

            MessageBoxGrid.Content = null;
            if (!marketApis.setActiveApiByName(Market.Name))
            {
                throw new Exception("No market API found that correspond to database market name.");
            }

            try
            {
                var accountBalance = await marketApis.ActiveMarketApi.GetAccountBalanceAsync();

                var ledgersDisplay = new List<LedgerDisplay>();
                var ledgers = await marketApis.ActiveMarketApi.GetLedgersAsync();

                foreach (var balance in accountBalance)
                {
                    var asset = await marketApis.ActiveMarketApi.GetAssetsAsync(new string[] { balance.Name! });
                    if (asset is not null)
                    {
                        balance.Name = asset[0].Symbol;
                        balance.TotalAmount = Math.Round(balance.TotalAmount, asset[0].DisplayDecimals);
                    }
                }

                foreach (var ledger in ledgers)
                {
                    var ledgerDisplay = new LedgerDisplay
                    {
                        ReferenceId = ledger.ReferenceId,
                        Date = ledger.Date.ToString("dd.MM.yyyy"),
                        Type = ledger.Type.Substring(0, 1).ToUpper() + ledger.Type.Substring(1),
                        Subtype = ledger.Subtype,
                        Amount = ledger.Amount.ToString(),
                        Fee = ledger.Fee.ToString(),
                        Balance = ledger.Balance.ToString()
                    };

                    var asset = await marketApis.ActiveMarketApi.GetAssetsAsync(new string[] { ledger.Asset });
                    if (asset is not null)
                    {
                        ledger.Asset = asset[0].Symbol;
                        string currencySymbol = CurrencyCodeMapper.GetSymbol(ledger.Asset);

                        ledgerDisplay.Amount = $"{ledgerDisplay.Amount} {currencySymbol}";
                        ledgerDisplay.Fee = $"{ledgerDisplay.Fee} {currencySymbol}";
                        ledgerDisplay.Balance = $"{ledgerDisplay.Balance} {currencySymbol}";

                        if (ledger.Amount > 0)
                        {
                            ledgerDisplay.Amount = "+" + ledgerDisplay.Amount;
                        }
                    }

                    ledgersDisplay.Add(ledgerDisplay);
                }

                AccountBalanceListBox.ItemsSource = accountBalance;
                TransactionsListBox.ItemsSource = ledgersDisplay;
            }
            catch (KrakenApiException krakenApiException)
            {
                MessageBoxGrid.Content = new FeedbackMessage(krakenApiException.Message);
            }
            finally
            {
                MarketsComboBox.IsEnabled = true;
            }
        }
    }
}
