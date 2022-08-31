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

                if (await db.SaveChangesAsync() == 0)
                {
                    throw new Exception("Cannot store initial configuration in the database.");
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
