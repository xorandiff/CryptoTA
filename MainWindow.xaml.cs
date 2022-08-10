using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTA
{
    public partial class MainWindow : Window
    {
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

        public string cryptocurrency = "ETH";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            statusText.Text = "Connecting to server...";
            cryptocurrency = "ETH";

            string uriString = "https://www.bitstamp.net/api/v2/ohlc/ethusd/?limit=1000&step=86400";
            Uri baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = client.Execute<BitstampOhlc>(request);

            if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
            {
                List<object> values = new List<object>();
                List<string> labels = new List<string>();

                foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();
                    values.Add(ohlcItem.Close);
                    labels.Add(dateTime.ToString("MM/dd/yyyy"));
                }

                chartControl.SeriesCollection[0].Values.AddRange(values);
                chartControl.Labels = labels;

                statusText.Text = "Data synchronized";

                FetchBitstampData();
            }
            else
            {
                statusText.Text = "Server closed connection";
            }
        }

        public async Task FetchBitstampData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                Uri baseUrl = new Uri("https://www.bitstamp.net/api/v2/ticker/" + cryptocurrency.ToLower() + "usd/");
                var client = new RestClient(baseUrl);
                var request = new RestRequest("get", Method.Get);

                var response = client.Execute<BitstampTick>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    currentPriceText.Text = response.Data.Last.ToString("C", CultureInfo.CreateSpecificCulture("en-us"));
                    currentChangeText.Text = response.Data.Percent_change_24.ToString() + "%";
                }
                else
                {
                    statusText.Text = "Server closed connection";
                }
            }
        }

        private void cryptocurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cryptocurrencyComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)cryptocurrencyComboBox.SelectedItem;
                cryptocurrency = (string)selectedItem.Content;
            }
        }
    }
}
