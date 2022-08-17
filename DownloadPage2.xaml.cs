using CryptoTA.Apis;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using CryptoTA.Database;
using CryptoTA.Database.Models;

namespace CryptoTA
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadPage1.xaml
    /// </summary>
    public partial class DownloadPage2 : Page
    {
        private readonly MarketApis marketApis;
        private List<(string, List<TradingPair>)> downloadedData;
        public DownloadPage2(MarketApis marketApisCollection)
        {
            InitializeComponent();
            marketApis = marketApisCollection;
            downloadedData = new List<(string, List<TradingPair>)>();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            SaveButton.Content = "Saving...";

            using (var db = new DatabaseContext())
            {
                foreach ((var marketName, var tradingPairs) in downloadedData)
                {
                    var market = new Market 
                    { 
                        Name = marketName, 
                        CredentialsRequired = false,
                        TradingPairs = tradingPairs
                    };
                    db.Markets.Add(market);
                }

                db.SaveChanges();
            }

            NavigationService.Navigate(new DownloadWindow(true));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int totalTradingPairsCount = 0;
                downloadedData = new List<(string, List<TradingPair>)>();
                foreach (var marketApi in marketApis)
                {
                    if (marketApi.Enabled)
                    {
                        var tradingPairs = await marketApi.GetTradingPairs();
                        if (tradingPairs != null)
                        {
                            downloadedData.Add((marketApi.Name, tradingPairs));
                            totalTradingPairsCount += tradingPairs.Count;
                        }
                    }
                }

                DoneLabel.Content = $"Done. Total trading pairs downloaded: {totalTradingPairsCount}";
                SaveButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
