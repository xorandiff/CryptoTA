using RestSharp;
using System;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Chart;
using System.Collections.Generic;
using CryptoTA.Indicators;

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
        private string statusText = "";

        public string StatusText { get => statusText; set => statusText = value; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.Markets.Any())
                {
                    var downloadWindow = new DownloadWindow();
                    downloadWindow.ShowDialog();
                }
            }

            _ = FetchTickData();
        }

        public async Task FetchTickData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
                {
                    var tickData = await marketApis.ActiveMarketApi.GetTick(tradingPair);
                    var regionInfo = new RegionInfo(tradingPair.CounterSymbol);
                    CurrentPriceText.Text = regionInfo.CurrencySymbol + " " + tickData.Close;
                }


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

        public void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is Market market)
            {
                marketApis.setActiveApiByName(market.Name);
            }
        }

        public async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
            {
                await LoadChartData();
            }
        }

        public async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is ChartTimeSpan timeInterval)
            {
                await LoadChartData(DateTime.Now.AddSeconds(-1 * timeInterval.Value));
            }
        }

        private async Task LoadChartData(DateTime? startDate = null)
        {
            if (TradingPairComboBox.SelectedItem is not TradingPair tradingPair)
            {
                throw new Exception("No trading pair selected.");
            }

            using DatabaseContext db = new();
            if (db.TradingPairs.Find(tradingPair.TradingPairId) is TradingPair dbTradingPair)
            {
                if (!dbTradingPair.Ticks.Any())
                {
                    var ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                    tradingPair.Ticks.AddRange(ticks);

                    await db.SaveChangesAsync();
                }

                if (startDate != null)
                {
                    tradingPair = db.TradingPairs.Find(tradingPair.TradingPairId);
                    var ticksBeforeStartDate = tradingPair.Ticks.Any(tick => tick.Date < startDate);
                    if (!ticksBeforeStartDate)
                    {

                        var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
                        var oldestStoredDate = tradingPair.Ticks.Select(tick => tick.Date).Min();

                        while (oldestStoredDate > startDate)
                        {
                            oldestStoredDate = oldestStoredDate.AddSeconds(-1 * maxTimeInterval);
                            var ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                            tradingPair.Ticks.AddRange(ticks);

                            await db.SaveChangesAsync();
                        }
                    }
                }

                if (startDate == null)
                {
                    startDate = DateTime.Now.AddSeconds(-1 * marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval);
                }

                var timeFormat = "dd.MM.yyyy";
                if (DateTime.Now.AddDays(-4) < startDate)
                {
                    timeFormat = "HH:mm";
                }
                else if (DateTime.Now.AddDays(-15) < startDate)
                {
                    timeFormat = "dd.MM.yyyy HH:mm";
                }

                tradingPair = db.TradingPairs.Find(tradingPair.TradingPairId);
                var filteredTicks = tradingPair.Ticks.Where(tick => tick.Date >= startDate).ToList();

                MovingAveragesItemsControl.ItemsSource = movingAverages.Run(filteredTicks);

                var values = filteredTicks.Select(tick => (object)tick.Close).ToList();
                var labels = filteredTicks.Select(tick => tick.Date.ToString(timeFormat));

                if (values.Count > 500)
                {
                    int nthSkipValue = values.Count / 500;
                    values = values.Where((x, i) => i % nthSkipValue == 0).ToList();
                    labels = labels.Where((x, i) => i % nthSkipValue == 0).ToList();
                }

                if (ChartControl != null)
                {
                    ChartControl.Series[0].Values.Clear();
                    //chartLabels.Clear();

                    ChartControl.Series[0].Values.AddRange(values);
                    foreach (var label in labels)
                    {
                        //chartLabels.Add(label);
                    }
                }
            }
        }
    }
}
