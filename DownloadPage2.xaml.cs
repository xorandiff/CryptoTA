using CryptoTA.Apis;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using System.Linq;

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

                if (!db.TimeIntervals.Any())
                {
                    List<TimeInterval> timeIntervals = new()
                    {
                        new TimeInterval { Name = "1 day", Seconds = 86400 },
                        new TimeInterval { Name = "3 days", Seconds = 86400 * 3 },
                        new TimeInterval { Name = "1 week", Seconds = 86400 * 7 },
                        new TimeInterval { Name = "2 weeks", Seconds = 86400 * 14 },
                        new TimeInterval { Name = "1 month", Seconds = 86400 * 31 },
                        new TimeInterval { Name = "3 months", Seconds = 86400 * 31 * 3 },
                        new TimeInterval { Name = "6 months", Seconds = 86400 * 31 * 6 },
                        new TimeInterval { Name = "1 year", Seconds = 86400 * 31 * 12 },
                        new TimeInterval { Name = "5 years", Seconds = 86400 * 31 * 12 * 5 }
                    };
                    db.TimeIntervals.AddRange(timeIntervals);
                }

                var entriesWritten = db.SaveChanges();
                if (entriesWritten == 0)
                {
                    throw new Exception("Cannot store initial dataset in database.");
                }
            }

            using (var db = new DatabaseContext())
            {
                if (!db.Settings.Any())
                {
                    var settings = new Settings
                    {
                        TradingPair = db.TradingPairs.First(),
                        TimeInterval = db.TimeIntervals.First()
                    };
                    db.Settings.Add(settings);
                }

                var entriesWritten = db.SaveChanges();
                if (entriesWritten == 0)
                {
                    throw new Exception("Cannot store initial configuration in the database.");
                }
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
