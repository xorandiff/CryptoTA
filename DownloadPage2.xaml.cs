using CryptoTA.Apis;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CryptoTA
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadPage1.xaml
    /// </summary>
    public partial class DownloadPage2 : Page
    {
        private readonly MarketApis marketApis;
        public DownloadPage2(MarketApis marketApisCollection)
        {
            InitializeComponent();
            marketApis = marketApisCollection;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //this.NavigationService.Navigate()
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int totalTradingPairsCount = 0;
                var downloadedData = new List<(string, List<TradingPair>)>();
                foreach (var marketApi in marketApis)
                {
                    if (marketApi.Enabled)
                    {
                        var tradingPairs = (List<TradingPair>) await marketApi.GetTradingPairs();
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
