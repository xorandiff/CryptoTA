using RestSharp;
using System;
using System.Reflection;
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
            public double Open24 { get; set; }
            public double PercentChange24 { get; set; }

        }

        public string cryptocurrency = "ETH";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            statusText.Text = "Connecting to server...";
            cryptocurrency = (string)(cryptocurrencyComboBox.SelectedItem as PropertyInfo).GetValue("");
            FetchBitstampData();
        }

        public async Task FetchBitstampData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                Uri baseUrl = new Uri("https://www.bitstamp.net/api/v2/ticker/" + cryptocurrency + "usd/");
                var client = new RestClient(baseUrl);
                var request = new RestRequest("get", Method.Get);

                var response = client.Execute<BitstampTick>(request);

                if (response.IsSuccessful)
                {
                    var dt = DateTime.Now;
                    string currentTimeString = dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                    chartControl.SeriesCollection[0].Values.Add(response.Data.Last);
                    chartControl.Labels.Add(currentTimeString);
                    statusText.Text = "Data synchronized";
                }
                else
                {
                    statusText.Text = "Server closed connection";
                }
            }
        }

        private void cryptocurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //cryptocurrency = cryptocurrencyComboBox.SelectedItem.ToString();
        }
    }
}
