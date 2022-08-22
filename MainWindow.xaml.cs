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
using CryptoTA.Database.Models;
using CryptoTA.Chart;
using CryptoTA.Services;
using System.Collections.ObjectModel;

namespace CryptoTA
{
    public partial class MainWindow : Window
    {
        private MarketApis marketApis = new();
        private Market selectedMarket = new();
        private TradingPair selectedTradingPair = new();
        private int selectedTradingPairId = 0;

        public List<TradingPair> TradingPairs
        {
            get => selectedMarket.TradingPairs ?? new List<TradingPair>();
        }

        public TradingPair SelectedTradingPair
        {
            get { return selectedTradingPair; } 
            set { selectedTradingPair = value; }
        }

        public ObservableCollection<OhlcSeriesItem> OhlcSeriesItems { get; set; }

        public string StatusText { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            OhlcSeriesItems = new();
            StatusText = "Checking database...";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<TradingPair>? list;

            using (var db = new DatabaseContext())
            {
                if (!db.Markets.Any())
                {
                    var downloadWindow = new DownloadWindow();
                    downloadWindow.ShowDialog();
                }

                selectedMarket = db.Markets.First();
                selectedTradingPair = selectedMarket.TradingPairs.First();
                selectedTradingPairId = selectedTradingPair.TradingPairId;
                list = selectedMarket.TradingPairs.ToList();

            }

            marketApis.setActiveApiByName(selectedMarket.Name);

            _ = FetchTickData();
            _ = LoadChartData();
        }

        public async Task FetchTickData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                var tickData = await marketApis.ActiveMarketApi.GetTick(selectedTradingPair);
                currentPriceText.Text = tickData.Close.ToString("C", CultureInfo.CreateSpecificCulture(CurrencyDataService.CurrencyToCulture(selectedTradingPair.CounterSymbol)));
            }
        }

        private async Task LoadChartData(DateTime? startDate = null)
        {
            TradingPair? tradingPair;

            using (var db = new DatabaseContext())
            {
                tradingPair = db.TradingPairs.Find(selectedTradingPairId);
                if (tradingPair == null)
                {
                    throw new MissingMemberException();
                }

                if (tradingPair.Ticks.ElementAt(0) == null)
                {
                    StatusText = "Downloading initial data...";
                    var Ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                    tradingPair.Ticks.AddRange(Ticks);
                    db.SaveChanges();
                }
            }

            using (var db = new DatabaseContext())
            {
                if (startDate != null)
                {
                    var ticksBeforeStartDate = tradingPair.Ticks.Any(tick => tick.Date < startDate);
                    if (!ticksBeforeStartDate)
                    {
                        StatusText = "Downloading missing data...";
                        var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
                        var oldestStoredDate = tradingPair.Ticks.Select(tick => tick.Date).Min();

                        while (oldestStoredDate > startDate)
                        {
                            oldestStoredDate = oldestStoredDate.AddSeconds(-1 * maxTimeInterval);
                            var Ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());


                            tradingPair.Ticks.AddRange(Ticks);
                        }
                    }
                }

                db.SaveChanges();
            }

            StatusText = "Loading data from database...";

            if (startDate == null)
            {
                startDate = DateTime.Now.AddSeconds(-1 * marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval);
            }

            List<Tick>? values = new();

            using (var db = new DatabaseContext())
            {
                values = tradingPair.Ticks.Where(tick => tick.Date >= startDate).ToList();
            }

            OhlcSeriesItems.Clear();

            foreach (var tick in values)
            {
                OhlcSeriesItems.Add(new OhlcSeriesItem { Category = tradingPair.DisplayName, High = tick.High, Low = tick.Low, Open = tick.Open, Close = tick.Close });
            }

            selectedTradingPair = tradingPair;
            selectedTradingPairId = tradingPair.TradingPairId;

            StatusText = "Data synchronized";
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

        private void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                            TradingPairComboBox.InvalidateVisual();

                            _ = LoadChartData();
                        }
                    }
                }
            }
        }

        private void TimeSpanComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeSpanComboBox.SelectedValue != null)
            {
                var dateTime = DateTime.Now;
                var selectedTimeSpan = (uint)TimeSpanComboBox.SelectedValue;
                _ = LoadChartData(dateTime.AddSeconds(-1 * selectedTimeSpan));
            }
        }
    }
}
