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
using Newtonsoft.Json.Linq;

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
        private int selectedTradingPairId = 0;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
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
                selectedTradingPairId = selectedTradingPair.TradingPairId;
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

            await LoadChartData();
            FetchTickData();
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
            var request = new RestRequest(baseUrl, RestSharp.Method.Get);

            var response = await client.ExecuteAsync<ExchangeRateConvertResponse>(request);
            if (response.IsSuccessful && response.Data != null && response.Data.Success)
            {
                return response.Data.Result;
            }

            return 0d;
        }

        private async Task LoadChartData(DateTime? startDate = null)
        {
            using (var db = new DatabaseContext())
            {
                var tradingPair = db.TradingPairs.Find(selectedTradingPairId);
                if (tradingPair != null)
                {
                    if (tradingPair.Ticks.Count == 0)
                    {
                        statusText.Text = "Downloading initial data...";
                        var Ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                        tradingPair.Ticks.AddRange(Ticks);
                        await db.SaveChangesAsync();
                    }

                    if (startDate != null)
                    {
                        var ticksBeforeStartDate = tradingPair.Ticks.FindAll(tick => tick.Date < startDate).Count;
                        if (ticksBeforeStartDate == 0)
                        {
                            statusText.Text = "Downloading missing data...";
                            var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
                            var oldestStoredDate = tradingPair.Ticks.Select(tick => tick.Date).Min();

                            while (oldestStoredDate > startDate)
                            {
                                oldestStoredDate = oldestStoredDate.AddSeconds(-1 * maxTimeInterval);
                                var Ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                                tradingPair.Ticks.AddRange(Ticks);
                                await db.SaveChangesAsync();
                            }
                        }
                    }

                    statusText.Text = "Loading data from database...";

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

                    var values = tradingPair.Ticks.Where(tick => tick.Date >= startDate).Select(tick => (object)tick.Close).ToList();
                    var labels = tradingPair.Ticks.Where(tick => tick.Date >= startDate).Select(tick => tick.Date.ToString(timeFormat));
                    
                    if (values.Count > 500)
                    {
                        int nthSkipValue = values.Count / 500;
                        values = values.Where((x, i) => i % nthSkipValue == 0).ToList();
                        labels = labels.Where((x, i) => i % nthSkipValue == 0).ToList();
                    }

                    chartControl.SeriesCollection[0].Values.Clear();
                    chartControl.Labels.Clear();

                    chartControl.SeriesCollection[0].Values.AddRange(values);
                    chartControl.Labels.AddRange(labels);

                    selectedTradingPair = tradingPair;
                    selectedTradingPairId = tradingPair.TradingPairId;

                    statusText.Text = "Data synchronized";
                }
            }
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
                
                if (selectedItem != null && selectedItem.TradingPairId != selectedTradingPairId)
                {
                    using (var db = new DatabaseContext())
                    {
                        var newTradingPair = db.TradingPairs.Find(selectedItem.TradingPairId);
                        if (newTradingPair != null)
                        {
                            SelectedTradingPair = newTradingPair;
                            selectedTradingPairId = newTradingPair.TradingPairId;

                            currentCurrencyText.Text = "/" + newTradingPair.BaseSymbol;
                            TradingPairComboBox.InvalidateVisual();

                            await LoadChartData();
                        }
                    }
                }
            }
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
