using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public string cryptocurrency = "ETH";
        public string realCurrency = "USD";

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
            statusText.Text = "Downloading data...";
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
                    double currentValue = response.Data.Last * await getCurrencyRate("USD", realCurrency);
                    currentPriceText.Text = currentValue.ToString("C", CultureInfo.CreateSpecificCulture(currencyToCulture(realCurrency)));

                    float percents = response.Data.Percent_change_24;
                    string percentString = response.Data.Percent_change_24.ToString() + "%";

                    Color percentColor = Color.FromRgb(240, 240, 240);
                    if (percents > 0)
                    {
                        if (percents >= 15)
                        {
                            percentColor = Color.FromRgb(0, 255, 0);
                        }
                        else if (percents >= 10)
                        {
                            percentColor = Color.FromRgb(41, 179, 41);
                        }
                        else if (percents >= 5)
                        {
                            percentColor = Color.FromRgb(103, 165, 103);
                        }
                        else if (percents >= 1)
                        {
                            percentColor = Color.FromRgb(181, 255, 181);
                        }
                    } else if (percents < 0)
                    {
                        if (percents <= -15)
                        {
                            percentColor = Color.FromRgb(255, 0, 0);
                        }
                        else if (percents <= -10)
                        {
                            percentColor = Color.FromRgb(179, 41, 41);
                        }
                        else if (percents <= -5)
                        {
                            percentColor = Color.FromRgb(165, 103, 103);
                        }
                        else if (percents <= -1)
                        {
                            percentColor = Color.FromRgb(255, 181, 181);
                        }
                    }
                    currentChangeText.Foreground = new SolidColorBrush(percentColor);

                    if (percents > 0) {
                        percentString = "+" + percentString;
                    }
                    currentChangeText.Text = percentString;
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

        private async void realCurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (realCurrencyComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)realCurrencyComboBox.SelectedItem;
                string targetCurrency = (string)selectedItem.Content;

                if (targetCurrency != null)
                {
                    realCurrency = targetCurrency;

                    double currencyRate = await getCurrencyRate("USD", realCurrency);
                    chartControl.changeByRate(currencyRate, realCurrency, currencyToCulture(realCurrency));
                }
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

        private async void setChartTimeSpan(int limit, int timeSpanSeconds)
        {
            statusText.Text = "Downloading data...";

            string uriString = "https://www.bitstamp.net/api/v2/ohlc/ethusd/?limit=" + limit + "&step=" + timeSpanSeconds;
            Uri baseUrl = new Uri(uriString);
            var client = new RestClient(baseUrl);
            var request = new RestRequest(baseUrl, Method.Get);

            var response = await client.ExecuteAsync<BitstampOhlc>(request);

            if (response.IsSuccessful && response.Data != null && response.Data.Data != null && response.Data.Data.Ohlc != null)
            {
                List<object> values = new List<object>();
                List<string> labels = new List<string>();

                foreach (BitstampOhlcItem ohlcItem in response.Data.Data.Ohlc)
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(ohlcItem.Timestamp).ToLocalTime();
                    values.Add(ohlcItem.Close);
                    if (timeSpanSeconds < 300)
                    {
                        labels.Add(dateTime.ToString("HH:mm"));

                    }
                    else if (timeSpanSeconds < 86400)
                    {
                        labels.Add(dateTime.ToString("MM/dd/yyyy HH:mm"));
                    } else
                    {
                        labels.Add(dateTime.ToString("MM/dd/yyyy"));
                    }
                }

                chartControl.SeriesCollection[0].Values.Clear();
                chartControl.Labels.Clear();

                chartControl.SeriesCollection[0].Values.AddRange(values);
                chartControl.Labels.AddRange(labels);

                double currencyRate = await getCurrencyRate("USD", realCurrency);
                chartControl.changeByRate(currencyRate, realCurrency, currencyToCulture(realCurrency));

                statusText.Text = "Data synchronized";
            }
            else
            {
                statusText.Text = "Server closed connection";
            }
        }

        private void timeSpan0Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(1000, 259200);
        }

        private void timeSpan1Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(730, 43200);
        }

        private void timeSpan2Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(730, 21600);
        }

        private void timeSpan3Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(730, 14400);
        }

        private void timeSpan4Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(744, 3600);
        }

        private void timeSpan5Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(744, 1800);
        }

        private void timeSpan6Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(744, 900);
        }

        private void timeSpan7Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(864, 300);
        }

        private void timeSpan8Btn_Click(object sender, RoutedEventArgs e)
        {
            setChartTimeSpan(480, 180);
        }
    }
}
