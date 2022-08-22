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

        private MarketApis marketApis = new();
        private MovingAverages movingAverages = new();
        private List<Tick> chartTicks = new();
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

        public async Task FetchTickData()
        {
            //var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            //while (await periodicTimer.WaitForNextTickAsync())
            //{
            //    if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
            //    {
            //        var tickData = await marketApis.ActiveMarketApi.GetTick(tradingPair);
            //        CurrentPriceText.Text = CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + tickData.Close;
            //    }


            //    //float percents = response.Data.Percent_change_24;
            //    //string percentString = response.Data.Percent_change_24.ToString() + "%";

            //    //Color percentColor = Color.FromRgb(240, 240, 240);
            //    //if (percents > 0)
            //    //{
            //    //    if (percents >= 15)
            //    //    {
            //    //        percentColor = Color.FromRgb(0, 255, 0);
            //    //    }
            //    //    else if (percents >= 10)
            //    //    {
            //    //        percentColor = Color.FromRgb(41, 179, 41);
            //    //    }
            //    //    else if (percents >= 5)
            //    //    {
            //    //        percentColor = Color.FromRgb(103, 165, 103);
            //    //    }
            //    //    else if (percents >= 1)
            //    //    {
            //    //        percentColor = Color.FromRgb(181, 255, 181);
            //    //    }
            //    //} else if (percents < 0)
            //    //{
            //    //    if (percents <= -15)
            //    //    {
            //    //        percentColor = Color.FromRgb(255, 0, 0);
            //    //    }
            //    //    else if (percents <= -10)
            //    //    {
            //    //        percentColor = Color.FromRgb(179, 41, 41);
            //    //    }
            //    //    else if (percents <= -5)
            //    //    {
            //    //        percentColor = Color.FromRgb(165, 103, 103);
            //    //    }
            //    //    else if (percents <= -1)
            //    //    {
            //    //        percentColor = Color.FromRgb(255, 181, 181);
            //    //    }
            //    //}
            //    //currentChangeText.Foreground = new SolidColorBrush(percentColor);

            //    //if (percents > 0) {
            //    //    percentString = "+" + percentString;
            //    //}
            //    //currentChangeText.Text = percentString;
            //}
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
            if (MenuTabControl.SelectedItem == IndicatorsTabItem)
            {
                if (MovingAveragesItemsControl != null)
                {
                    statusText = "Computing indicators...";
                    MovingAveragesItemsControl.ItemsSource = movingAverages.Run(CurrencyChart.ChartTicks);
                    statusText = "";
                }
            }
        }
    }
}
