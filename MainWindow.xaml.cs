using RestSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CryptoTA.Apis;
using CryptoTA.Models;

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
        private Market selectedMarket = new();
        private TradingPair selectedTradingPair = new();
        public List<TradingPair> TradingPairs
        {
            get
            {
                return selectedMarket.TradingPairs ?? new List<TradingPair>();
            }
        }
        public TradingPair SelectedTradingPair
        {
            get { return selectedTradingPair; } 
            set { selectedTradingPair = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private string currencyToCulture(string currency)
        {
            if (currency == "USD")
            {
                return "en-us";
            }
            else if (currency == "PLN")
            {
                return "pl";
            }
            else if (currency == "GBP")
            {
                return "en-gb";
            }
            return "en-us";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                if (db.Markets.Count() == 0)
                {
                    var downloadWindow = new DownloadWindow();
                    downloadWindow.ShowDialog();
                }

                selectedMarket = db.Markets.First();
                selectedTradingPair = selectedMarket.TradingPairs.First();
                var list = selectedMarket.TradingPairs.ToList();

                marketComboBox.ItemsSource = marketApis;
                marketComboBox.DisplayMemberPath = "Name";
                marketComboBox.SelectedItem = marketApis.ActiveMarketApi;
                marketComboBox.InvalidateVisual();

                TradingPairComboBox.ItemsSource = list;
                TradingPairComboBox.DisplayMemberPath = "DisplayName";
                TradingPairComboBox.SelectedItem = list[0];
                TradingPairComboBox.InvalidateVisual();

                marketApis.setActiveApiByName(selectedMarket.Name);
            }

            currentCurrencyText.Text = "/" + selectedTradingPair.BaseSymbol;

            FetchTickData();
            LoadChartData();
        }

        public async Task FetchTickData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                var tickData = await marketApis.ActiveMarketApi.GetTick(selectedTradingPair);
                currentPriceText.Text = tickData.Close.ToString("C", CultureInfo.CreateSpecificCulture(currencyToCulture(selectedTradingPair.CounterSymbol)));

                //float percents = response.Data.Percent_change_24;
                //string percentString = response.Data.Percent_change_24.ToString() + "%";

                //Color percentColor = Color.FromRgb(240, 240, 240);
                //if (percents > 0)
                //{
                //    if (percents >= 15)
                //    {
                //        percentColor = Color.FromRgb(0, 255, 0);
                //    }
                //    else if (percents >= 10)
                //    {
                //        percentColor = Color.FromRgb(41, 179, 41);
                //    }
                //    else if (percents >= 5)
                //    {
                //        percentColor = Color.FromRgb(103, 165, 103);
                //    }
                //    else if (percents >= 1)
                //    {
                //        percentColor = Color.FromRgb(181, 255, 181);
                //    }
                //} else if (percents < 0)
                //{
                //    if (percents <= -15)
                //    {
                //        percentColor = Color.FromRgb(255, 0, 0);
                //    }
                //    else if (percents <= -10)
                //    {
                //        percentColor = Color.FromRgb(179, 41, 41);
                //    }
                //    else if (percents <= -5)
                //    {
                //        percentColor = Color.FromRgb(165, 103, 103);
                //    }
                //    else if (percents <= -1)
                //    {
                //        percentColor = Color.FromRgb(255, 181, 181);
                //    }
                //}
                //currentChangeText.Foreground = new SolidColorBrush(percentColor);

                //if (percents > 0) {
                //    percentString = "+" + percentString;
                //}
                //currentChangeText.Text = percentString;
            }
        }

        private async Task<double> getCurrencyRate(string sourceCurrency, string targetCurrency)
        {
            string uriString = "https://api.exchangerate.host/convert?from=" + sourceCurrency + "&to=" + targetCurrency;
            Uri baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<ExchangeRateConvertResponse>(request);
            if (response.IsSuccessful && response.Data != null && response.Data.Success)
            {
                return response.Data.Result;
            }

            return 0d;
        }

        private async Task LoadChartData()
        {
            using (var db = new DatabaseContext())
            {
                if (selectedTradingPair.Ticks.Count == 0)
                {
                    statusText.Text = "Downloading new data...";
                    var Ticks = await marketApis.ActiveMarketApi.GetOhlcData(selectedTradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                    selectedTradingPair.Ticks.AddRange(Ticks);
                    await db.SaveChangesAsync();
                }

                statusText.Text = "Loading data from database...";

                var values = selectedTradingPair.Ticks.Select(tick => (object)tick.Close).ToList();
                var labels = selectedTradingPair.Ticks.Select(tick => tick.Date.ToString("HH:mm"));

                chartControl.SeriesCollection[0].Values.Clear();
                chartControl.Labels.Clear();

                chartControl.SeriesCollection[0].Values.AddRange(values);
                chartControl.Labels.AddRange(labels);

                statusText.Text = "Data synchronized";
            }
        }

        private void timeSpan0Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 1000;
            //_timeInterval = 259200;
            //FetchChartData();
        }

        private void timeSpan1Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 730;
            //_timeInterval = 43200;
            //FetchChartData();
        }

        private void timeSpan2Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 730;
            //_timeInterval = 21600;
            //FetchChartData();
        }

        private void timeSpan3Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 730;
            //_timeInterval = 14400;
            //FetchChartData();
        }

        private void timeSpan4Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 744;
            //_timeInterval = 3600;
            //FetchChartData();
        }

        private void timeSpan5Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 1000;
            //_timeInterval = 259200;
            //FetchChartData();
        }

        private void timeSpan6Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 744;
            //_timeInterval = 900;
            //FetchChartData();
        }

        private void timeSpan7Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 864;
            //_timeInterval = 300;
            //FetchChartData();
        }

        private void timeSpan8Btn_Click(object sender, RoutedEventArgs e)
        {
            //_limit = 480;
            //_timeInterval = 180;
            //FetchChartData();
        }

        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var accountsWindow = new AccountsWindow();
            accountsWindow.Owner = this;
            accountsWindow.ShowDialog();
        }

        private void marketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem != null && SelectedTradingPair != null)
            {
                var selectedItem = (TradingPair) TradingPairComboBox.SelectedItem;
                
                if (selectedItem != null && selectedItem.TradingPairId != SelectedTradingPair.TradingPairId)
                {
                    using (var db = new DatabaseContext())
                    {
                        var newTradingPair = db.TradingPairs.Find(selectedItem.TradingPairId);
                        if (newTradingPair != null)
                        {
                            SelectedTradingPair = newTradingPair;
                        }
                    }

                    currentCurrencyText.Text = "/" + SelectedTradingPair.BaseSymbol;
                    TradingPairComboBox.InvalidateVisual();

                    await LoadChartData();
                }
            }
        }
    }
}
