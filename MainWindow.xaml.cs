using RestSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using System.Collections.Generic;
using CryptoTA.Indicators;
using CryptoTA.Utils;
using System.Collections.ObjectModel;
using CryptoTA.Pages;

namespace CryptoTA
{
    public partial class MainWindow : Window
    {
        public class ExchangeRateConvertResponse
        {
            public bool Success { get; set; }
            public bool Historical { get; set; }
            public double Result { get; set; }
            public DateTime Date { get; set; }
        }

        private string statusText = "";

        public string StatusText { get => statusText; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using var db = new DatabaseContext();
            if (!db.Settings.Any())
            {
                var downloadWindow = new DownloadWindow();
                downloadWindow.ShowDialog();
            }
        }

        private async Task<double> GetCurrencyRate(string sourceCurrency, string targetCurrency)
        {
            string uriString = "https://api.exchangerate.host/convert?from=" + sourceCurrency + "&to=" + targetCurrency;
            Uri baseUrl = new (uriString);
            RestClient client = new (baseUrl);
            RestRequest request = new (baseUrl, Method.Get);

            var response = await client.ExecuteAsync<ExchangeRateConvertResponse>(request);
            if (response.IsSuccessful && response.Data != null && response.Data.Success)
            {
                return response.Data.Result;
            }

            return 0d;
        }

        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var accountsWindow = new AccountsWindow
            {
                Owner = this
            };
            accountsWindow.ShowDialog();
        }

        private void MenuTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuTabControl.SelectedIndex == 1)
            {
                //IndicatorsPageFrame.Refresh();
            }
        }
    }
}
