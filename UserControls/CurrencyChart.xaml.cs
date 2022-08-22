using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CryptoTA.Apis;
using CryptoTA.Database.Models;
using CryptoTA.Database;
using LiveCharts;
using LiveCharts.Wpf;
using CryptoTA.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryptoTA.UserControls
{
    public partial class CurrencyChart : UserControl
    {
        private readonly MarketApis marketApis = new();
        private string chartTitle = "";
        private Func<double, string> chartYFormatter = value => value.ToString();

        private List<Tick> chartTicks = new();

        private ObservableCollection<Market> markets = new();
        private ObservableCollection<TradingPair> tradingPairs = new();
        private ObservableCollection<TimeInterval> timeIntervals = new();
        private ObservableCollection<string> chartLabels = new();
        private SeriesCollection chartSeriesCollection = new();

        public CurrencyChart()
        {
            InitializeComponent();
            DataContext = this;
            try
            {
                using (var db = new DatabaseContext())
                if (!db.Settings.Any())
                {
                    throw new Exception("Configuration data missing.");
                }

                markets = CreateMarkets();
                Market = GetMarketFromSettings();
                Market = markets.Where(m => m.MarketId == Market.MarketId).First();

                bool marketApiFound = marketApis.setActiveApiByName(Market.Name);
                if (!marketApiFound)
                {
                    throw new Exception("No market API found that correspond to database market name.");
                }

                tradingPairs = CreateTradingPairs();
                TradingPair = GetTradingPairFromSettings();
                TradingPair = tradingPairs.Where(tp => tp.TradingPairId == TradingPair.TradingPairId).First();

                timeIntervals = CreateTimeIntervals();
                TimeInterval = GetTimeIntervalFromSettings();
                TimeInterval = timeIntervals.Where(ti => ti.TimeIntervalId == TimeInterval.TimeIntervalId).First();

                chartLabels = CreateChartLabels();
                chartSeriesCollection = CreateChartSeriesCollection();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public Market Market { get; set; } = GetMarketFromSettings();
        public TradingPair TradingPair { get; set; } = GetTradingPairFromSettings();
        public TimeInterval TimeInterval { get; set; } = GetTimeIntervalFromSettings();
        public string ChartTitle { get => chartTitle; }
        public SeriesCollection ChartSeriesCollection { get => chartSeriesCollection; }

        public Func<double, string> ChartYFormatter { get => chartYFormatter; }

        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }

        public ObservableCollection<Market> Markets { get => markets; }

        public ObservableCollection<TimeInterval> TimeIntervals { get => timeIntervals; }

        public ObservableCollection<string> ChartLabels { get => chartLabels; }

        private static Market GetMarketFromSettings()
        {
            using (var db = new DatabaseContext())
            {
                var settings = db.Settings.First();
                var dbTradingPair = db.TradingPairs.Find(settings.TradingPairId);

                if (dbTradingPair != null)
                {
                    var dbMarket = db.Markets.Find(dbTradingPair.MarketId);

                    if (dbMarket != null)
                    {
                        return dbMarket;
                    }
                    else
                    {
                        throw new Exception("Stored trading pair in Settings table has no corresponding Market assigned in database.");
                    }
                }
                else
                {
                    throw new Exception("Couldn't find stored trading pair in Settings table.");
                }
            }
        }

        private static TradingPair GetTradingPairFromSettings()
        {
            using (var db = new DatabaseContext())
            {
                var settings = db.Settings.First();
                var dbTradingPair = db.TradingPairs.Find(settings.TradingPairId);

                if (dbTradingPair != null)
                {
                    return dbTradingPair;
                }
                else
                {
                    throw new Exception("Couldn't find stored trading pair in Settings table.");
                }
            }
        }

        private static TimeInterval GetTimeIntervalFromSettings()
        {
            using (var db = new DatabaseContext())
            {
                var settings = db.Settings.Include("TimeInterval").First();
                if (settings.TimeInterval is TimeInterval dbTimeInterval)
                {
                    return dbTimeInterval;
                }
                else
                {
                    throw new Exception("Couldn't find stored time interval in Settings table.");
                }
            }
        }

        private static ObservableCollection<Market> CreateMarkets()
        {
            ObservableCollection<Market> markets = new();

            using var db = new DatabaseContext();
            var marketList = db.Markets.ToList();

            foreach (var market in marketList)
            {
                markets.Add(market);
            }

            return markets;
        }

        private ObservableCollection<TradingPair> CreateTradingPairs()
        {
            ObservableCollection<TradingPair> tradingPairs = new();

            using (var db = new DatabaseContext())
            {
                foreach (TradingPair tradingPair in db.TradingPairs.Where(tp => tp.MarketId == Market.MarketId).ToList())
                {
                    tradingPairs.Add(tradingPair);
                }
            }
            return tradingPairs;
        }

        private static ObservableCollection<TimeInterval> CreateTimeIntervals()
        {
            var timeIntervals = new ObservableCollection<TimeInterval>();

            using (var db = new DatabaseContext())
            {
                foreach (var timeInterval in db.TimeIntervals.ToList())
                {
                    timeIntervals.Add(timeInterval);
                }
            }

            return timeIntervals;
        }

        private static ObservableCollection<string> CreateChartLabels()
        {
            ObservableCollection<string> chartLabels = new();

            return chartLabels;
        }

        private SeriesCollection CreateChartSeriesCollection()
        {
            chartSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = TradingPair.BaseSymbol + "/" + TradingPair.CounterSymbol,
                    Values = new ChartValues<double> { },
                    PointGeometry = null
                }
            };

            return chartSeriesCollection;
        }

        private void EnableFilterComboBoxes()
        {
            MarketComboBox.IsEnabled = true;
            TradingPairComboBox.IsEnabled = true;
            TimeIntervalComboBox.IsEnabled = true;
        }

        private void DisableFilterComboBoxes()
        {
            MarketComboBox.IsEnabled = false;
            TradingPairComboBox.IsEnabled = false;
            TimeIntervalComboBox.IsEnabled = false;
        }


        private void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is Market market)
            {
                marketApis.setActiveApiByName(market.Name);
                using (var db = new DatabaseContext())
                {
                    var dbFirstTradingPair = db.TradingPairs.Where(tp => tp.MarketId == market.MarketId).First();

                    if (dbFirstTradingPair != null)
                    {
                        var settings = db.Settings.First();
                        settings.TradingPairId = dbFirstTradingPair.TradingPairId;

                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Selected market has no trading pairs available.");
                    }
                }
            }
        }

        private async void TradingPairComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
            {
                using (var db = new DatabaseContext())
                {
                    var settings = db.Settings.First();
                    settings.TradingPairId = tradingPair.TradingPairId;

                    db.SaveChanges();
                }

                await LoadChartData();
            }
        }

        private async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is TimeInterval timeInterval)
            {
                using (var db = new DatabaseContext())
                {
                    var settings = db.Settings.First();
                    settings.TimeIntervalId = timeInterval.TimeIntervalId;

                    db.SaveChanges();
                }

                await LoadChartData(DateTime.Now.AddSeconds(-timeInterval.Seconds));
            }
        }

        private async Task LoadChartData(DateTime? startDate = null)
        {
            try
            {
                DisableFilterComboBoxes();
                using DatabaseContext db = new();
                if (await db.TradingPairs.FindAsync(TradingPair.TradingPairId) is TradingPair dbTradingPair)
                {
                    await db.Entry(dbTradingPair).Collection(t => t.Ticks).LoadAsync();

                    if (!dbTradingPair.Ticks.Any())
                    {
                        var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());

                        dbTradingPair.Ticks.AddRange(ticks);

                        await db.SaveChangesAsync();
                    }

                    if (startDate == null)
                    {
                        startDate = DateTime.Now.AddSeconds(-marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval);
                    }

                    dbTradingPair = await db.TradingPairs.FindAsync(dbTradingPair.TradingPairId);
                    await db.Entry(dbTradingPair).Collection(t => t.Ticks).LoadAsync();

                    if (!dbTradingPair.Ticks.Any(tick => tick.Date < startDate))
                    {
                        var maxTimeInterval = marketApis.ActiveMarketApi.OhlcMaxDensityTimeInterval;
                        var oldestStoredDate = dbTradingPair.Ticks.Select(tick => tick.Date).Min();

                        while (oldestStoredDate >= startDate)
                        {
                            oldestStoredDate = oldestStoredDate.AddSeconds(-maxTimeInterval);

                            var ticks = await marketApis.ActiveMarketApi.GetOhlcData(dbTradingPair, oldestStoredDate, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());
                            dbTradingPair.Ticks.AddRange(ticks);

                        }

                        await db.SaveChangesAsync();
                    }

                    var currentDate = DateTime.Now;
                    var timeFormat = "dd.MM.yyyy";
                    if (currentDate.AddDays(-4) < startDate)
                    {
                        timeFormat = "HH:mm";
                    }
                    else if (currentDate.AddDays(-15) < startDate)
                    {
                        timeFormat = "dd.MM.yyyy HH:mm";
                    }

                    dbTradingPair = await db.TradingPairs.FindAsync(dbTradingPair.TradingPairId);
                    await db.Entry(dbTradingPair).Collection(t => t.Ticks).LoadAsync();

                    chartTicks = dbTradingPair.Ticks.Where(tick => tick.Date >= startDate).ToList();

                    var values = chartTicks.Select(tick => tick.Close).ToArray();
                    var labels = chartTicks.Select(tick => tick.Date.ToString(timeFormat)).ToArray();

                    if (values.Length > 500)
                    {
                        int nthSkipValue = values.Length / 500;
                        values = values.Where((x, i) => i % nthSkipValue == 0).ToArray();
                        labels = labels.Where((x, i) => i % nthSkipValue == 0).ToArray();
                    }
                    chartSeriesCollection[0].Values = new ChartValues<double>(values);

                    chartLabels.Clear();
                    foreach (var label in labels)
                    {
                        chartLabels.Add(label);
                    }

                    chartYFormatter = value => CurrencyCodeMapper.GetSymbol(dbTradingPair.CounterSymbol) + " " + value;
                    chartTitle = dbTradingPair.CounterSymbol + " price for 1 " + dbTradingPair.BaseSymbol;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                EnableFilterComboBoxes();
            }
        }
    }
}
