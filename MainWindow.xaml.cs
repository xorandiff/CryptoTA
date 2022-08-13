using RestSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public class BitstampTick
        {
            public double Last { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Vwap { get; set; }
            public double Volume { get; set; }
            public double Bid { get; set; }
            public double Ask { get; set; }
            public double Timestamp { get; set; }
            public double Open { get; set; }
            public double Open_24 { get; set; }
            public float Percent_change_24 { get; set; }

        }

        public class BitstampOhlcItem
        {
            public double High { get; set; }
            public long Timestamp { get; set; }
            public double Volume { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public double Open { get; set; }
        }

        public class BitstampOhlcData
        {
            public string? Pair { get; set; }
            public BitstampOhlcItem[]? Ohlc { get; set; }
        }

        public class BitstampOhlc
        {
            public BitstampOhlcData? Data { get; set; }
        }

        private string _cryptoCurrency = "ETH";
        private string _realCurrency = "USD";
        private uint _limit = 480;
        private uint _timeInterval = 180;

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

            statusText.Text = "Downloading data...";
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

            }
        }

        //private async Task<bool> FetchChartData(uint? limit = null, uint? timeInterval = null, string? cryptoCurrency = null, string? realCurrency = null)
        //{
        //    if (limit == null)
        //    {
        //        limit = _limit;
        //    }

        //    if (timeInterval == null)
        //    {
        //        timeInterval = _timeInterval;
        //    }

        //    if (cryptoCurrency == null)
        //    {
        //        cryptoCurrency = _cryptoCurrency;
        //    }

        //    if (realCurrency == null)
        //    {
        //        realCurrency = _realCurrency;
        //    }

        //    string uriString = "https://www.bitstamp.net/api/v2/ohlc/";
        //    uriString += cryptoCurrency.ToLower() + "usd";
        //    uriString += "/?limit=" + limit + "&step=" + timeInterval;

        //    Uri baseUrl = new Uri(uriString);
        //    var client = new RestClient(baseUrl);
        //    var request = new RestRequest(baseUrl, RestSharp.Method.Get);

        //    var response = await client.ExecuteAsync<BitstampOhlc>(request);

        //    if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
        //    {
        //        List<object> values = new List<object>();
        //        List<string> labels = new List<string>();

        //        foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
        //        {
        //            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //            dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();
        //            values.Add(ohlcItem.Close);

        //            string timeFormat;
        //            if (timeInterval < 300)
        //            {
        //                timeFormat = "HH:mm";
        //            }
        //            else if (timeInterval < 86400)
        //            {
        //                timeFormat = "MM/dd/yyyy HH:mm";
        //            }
        //            else
        //            {
        //                timeFormat = "MM/dd/yyyy";
        //            }
        //            labels.Add(dateTime.ToString(timeFormat));
        //        }

        //        chartControl.SeriesCollection[0].Values.Clear();
        //        chartControl.Labels.Clear();

        //        chartControl.SeriesCollection[0].Values.AddRange(values);
        //        chartControl.Labels.AddRange(labels);

        //        double currencyRate = await getCurrencyRate("USD", _realCurrency);
        //        chartControl.changeByRate(currencyRate, _realCurrency, currencyToCulture(_realCurrency));

        //        statusText.Text = "Data synchronized";

        //        return true;
        //    }
        //    else
        //    {
        //        statusText.Text = "Server closed connection";
        //        return false;
        //    }
        //}

        private void timeSpan0Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 1000;
            _timeInterval = 259200;
            //FetchChartData();
        }

        private void timeSpan1Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 730;
            _timeInterval = 43200;
            //FetchChartData();
        }

        private void timeSpan2Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 730;
            _timeInterval = 21600;
            //FetchChartData();
        }

        private void timeSpan3Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 730;
            _timeInterval = 14400;
            //FetchChartData();
        }

        private void timeSpan4Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 744;
            _timeInterval = 3600;
            //FetchChartData();
        }

        private void timeSpan5Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 1000;
            _timeInterval = 259200;
            //FetchChartData();
        }

        private void timeSpan6Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 744;
            _timeInterval = 900;
            //FetchChartData();
        }

        private void timeSpan7Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 864;
            _timeInterval = 300;
            //FetchChartData();
        }

        private void timeSpan8Btn_Click(object sender, RoutedEventArgs e)
        {
            _limit = 480;
            _timeInterval = 180;
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

        private void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem != null && currentCurrencyText != null)
            {
                var selectedItem = (TradingPair) TradingPairComboBox.SelectedItem;
                selectedTradingPair = selectedItem;
                currentCurrencyText.Text = "/" + selectedTradingPair.BaseSymbol;
                TradingPairComboBox.InvalidateVisual();

                //fetchChartData();
            }
        }
    }
}
