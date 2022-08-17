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
using CryptoTA.Database;

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
        private string statusText = "";

        public string StatusText { get => statusText; set => statusText = value; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.Markets.Any())
                {
                    var downloadWindow = new DownloadWindow();
                    downloadWindow.ShowDialog();
                }
            }

            //currentCurrencyText.Text = "/" + selectedTradingPair.BaseSymbol;

            await LoadChartData();
            _ = FetchTickData();
        }

        public async Task FetchTickData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                //var tickData = await marketApis.ActiveMarketApi.GetTick(selectedTradingPair);

                //var regionInfo = new RegionInfo(selectedTradingPair.CounterSymbol);
                //currentPriceText.Text = regionInfo.CurrencySymbol + " " + tickData.Close;

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

        private async Task LoadChartData(DateTime? startDate = null)
        {
            //using DatabaseContext db = new();
            //var tradingPair = db.TradingPairs.Find(selectedTradingPairId);
            //if (tradingPair != null)
            //{
            //    if (!tradingPair.Ticks.Any())
            //    {
            //        StatusText = "Downloading initial data...";

            //        var ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
            //        tradingPair.Ticks.AddRange(ticks);

            //        await db.SaveChangesAsync();
            //    }

            //    if (startDate != null)
            //    {
            //        var ticksBeforeStartDate = tradingPair.Ticks.FindAll(tick => tick.Date < startDate).Any();
            //        if (!ticksBeforeStartDate)
            //        {
            //            StatusText = "Downloading missing data...";
            //            var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
            //            var oldestStoredDate = tradingPair.Ticks.Select(tick => tick.Date).Min();

            //            while (oldestStoredDate > startDate)
            //            {
            //                oldestStoredDate = oldestStoredDate.AddSeconds(-1 * maxTimeInterval);
            //                var ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
            //                tradingPair.Ticks.AddRange(ticks);

            //                await db.SaveChangesAsync();
            //            }
            //        }
            //    }

            //    StatusText = "Loading data from database...";

            //    if (startDate == null)
            //    {
            //        startDate = DateTime.Now.AddSeconds(-1 * marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval);
            //    }

            //    var timeFormat = "dd.MM.yyyy";
            //    if (DateTime.Now.AddDays(-4) < startDate)
            //    {
            //        timeFormat = "HH:mm";
            //    }
            //    else if (DateTime.Now.AddDays(-15) < startDate)
            //    {
            //        timeFormat = "dd.MM.yyyy HH:mm";
            //    }

            //    var filteredTicks = tradingPair.Ticks.Where(tick => tick.Date >= startDate).ToList();

            //    var values = filteredTicks.Select(tick => (object)tick.Close).ToList();
            //    var labels = filteredTicks.Select(tick => tick.Date.ToString(timeFormat));

            //    if (values.Count > 500)
            //    {
            //        int nthSkipValue = values.Count / 500;
            //        values = values.Where((x, i) => i % nthSkipValue == 0).ToList();
            //        labels = labels.Where((x, i) => i % nthSkipValue == 0).ToList();
            //    }

            //    chartControl.SeriesCollection[0].Values.Clear();
            //    chartControl.Labels.Clear();

            //    chartControl.SeriesCollection[0].Values.AddRange(values);
            //    chartControl.Labels.AddRange(labels);

            //    selectedTradingPair = tradingPair;
            //    selectedTradingPairId = tradingPair.TradingPairId;

            //    StatusText = "Data synchronized";
            //}
        }

        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var accountsWindow = new AccountsWindow
            {
                Owner = this
            };
            accountsWindow.ShowDialog();
        }

        private void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (TradingPairComboBox.SelectedItem != null && SelectedTradingPair != null)
            //{
            //    var selectedItem = (TradingPair) TradingPairComboBox.SelectedItem;
                
            //    if (selectedItem != null && selectedItem.TradingPairId != selectedTradingPairId)
            //    {
            //        using (var db = new DatabaseContext())
            //        {
            //            var newTradingPair = db.TradingPairs.Find(selectedItem.TradingPairId);
            //            if (newTradingPair != null)
            //            {
            //                SelectedTradingPair = newTradingPair;
            //                selectedTradingPairId = newTradingPair.TradingPairId;

            //                currentCurrencyText.Text = "/" + newTradingPair.BaseSymbol;
            //                TradingPairComboBox.InvalidateVisual();

            //                await LoadChartData();
            //            }
            //        }
            //    }
            //}
        }

        private async void TimeSpanComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeSpanComboBox.SelectedValue != null)
            {
                var dateTime = DateTime.Now;
                var selectedTimeSpan = (uint)TimeSpanComboBox.SelectedValue;
                await LoadChartData(dateTime.AddSeconds(-1 * selectedTimeSpan));
            }
        }
    }
}
