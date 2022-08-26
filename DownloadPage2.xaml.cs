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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            SaveButton.Content = "Saving...";

            using (var db = new DatabaseContext())
            {
                foreach ((var marketName, var tradingPairs) in downloadedData)
                {
                    await db.Markets.AddAsync(new Market
                    {
                        Name = marketName,
                        CredentialsRequired = false,
                        TradingPairs = tradingPairs
                    });
                }

                if (!db.TimeIntervals.Any())
                {
                    await db.TimeIntervals.AddRangeAsync(new List<TimeInterval>()
                    {
                        new() { Name = "1 day", Seconds = 86400, IsIndicatorInterval = false },
                        new() { Name = "3 days", Seconds = 86400 * 3, IsIndicatorInterval = false },
                        new() { Name = "1 week", Seconds = 86400 * 7, IsIndicatorInterval = false },
                        new() { Name = "2 weeks", Seconds = 86400 * 14, IsIndicatorInterval = false },
                        new() { Name = "1 month", Seconds = 86400 * 31, IsIndicatorInterval = false },
                        new() { Name = "3 months", Seconds = 86400 * 31 * 3, IsIndicatorInterval = false },
                        new() { Name = "6 months", Seconds = 86400 * 31 * 6, IsIndicatorInterval = false },
                        new() { Name = "1 year", Seconds = 86400 * 31 * 12, IsIndicatorInterval = false },
                        new() { Name = "5 years", Seconds = 86400 * 31 * 12 * 5, IsIndicatorInterval = false },

                        new() { Name = "1 minute", Seconds = 60, IsIndicatorInterval = true },
                        new() { Name = "5 minutes", Seconds = 60 * 5, IsIndicatorInterval = true },
                        new() { Name = "15 minutes", Seconds = 60 * 15, IsIndicatorInterval = true },
                        new() { Name = "30 minutes", Seconds = 60 * 30, IsIndicatorInterval = true },
                        new() { Name = "1 hour", Seconds = 60 * 60, IsIndicatorInterval = true },
                        new() { Name = "2 hours", Seconds = 60 * 60 * 2, IsIndicatorInterval = true },
                        new() { Name = "4 hours", Seconds = 60 * 60 * 4, IsIndicatorInterval = true },
                        new() { Name = "1 day", Seconds = 60 * 60 * 24, IsIndicatorInterval = true },
                        new() { Name = "1 week", Seconds = 60 * 60 * 24 * 7, IsIndicatorInterval = true },
                        new() { Name = "1 month", Seconds = 60 * 60 * 24 * 31, IsIndicatorInterval = true }
                    });
                }

                if (await db.SaveChangesAsync() == 0)
                {
                    throw new Exception("Cannot store initial dataset in database.");
                }
            }

            using (var db = new DatabaseContext())
            {
                if (!db.Settings.Any())
                {
                    await db.Settings.AddAsync(new Settings
                    {
                        TradingPair = db.TradingPairs.First(),
                        TimeIntervalIdChart = db.TimeIntervals.Where(ti => !ti.IsIndicatorInterval).First().TimeIntervalId,
                        TimeIntervalIdIndicators = db.TimeIntervals.Where(ti => ti.IsIndicatorInterval).First().TimeIntervalId
                    });
                }

                if (await db.SaveChangesAsync() == 0)
                {
                    throw new Exception("Cannot store initial configuration in the database.");
                }
            }

            var parent = (DownloadWindow)Parent;
            parent.Close();
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
                        if (await marketApi.GetTradingPairs() is List<TradingPair> tradingPairs)
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
