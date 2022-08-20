using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace CryptoTA.UserControls
{
    public partial class CurrencyChart : UserControl
    {
        private readonly MarketApis marketApis = new();
        private Market market;
        private TradingPair tradingPair;
        private TimeInterval timeInterval;
        private string chartTitle;
        private Func<double, string> chartYFormatter;

        private List<Tick> chartTicks = new();

        private ObservableCollection<Market> markets;
        private ObservableCollection<TradingPair> tradingPairs;
        private ObservableCollection<TimeInterval> timeIntervals;
        private ObservableCollection<string> chartLabels;
        private SeriesCollection chartSeriesCollection;

        public CurrencyChart()
        {
            InitializeComponent();
            DataContext = this;
            try
            {
                using (var db = new DatabaseContext())
                if (!db.Configuration.Any())
                {
                    throw new Exception("Configuration data missing.");
                }

                markets = CreateMarkets();
                market = GetMarketFromSettings();

                bool marketApiFound = marketApis.setActiveApiByName(market.Name);
                if (!marketApiFound)
                {
                    throw new Exception("No market api found that correspond to database market name.");
                }

                tradingPairs = CreateTradingPairs();
                tradingPair = GetTradingPairFromSettings();

                timeIntervals = CreateTimeIntervals();
                timeInterval = GetTimeIntervalFromSettings();

                chartLabels = CreateChartLabels();
                chartSeriesCollection = CreateChartSeriesCollection();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static Market GetMarketFromSettings()
        {
            using (var db = new DatabaseContext())
            {
                var settings = db.Configuration.First();
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
                var settings = db.Configuration.First();
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
                var settings = db.Configuration.Include("TimeInterval").First();
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

            market ??= GetMarketFromSettings();

            using (var db = new DatabaseContext())
            {
                foreach (TradingPair tradingPair in db.Markets.Find(market.MarketId).TradingPairs.ToList())
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
            tradingPair ??= GetTradingPairFromSettings();

            chartSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = tradingPair.BaseSymbol + "/" + tradingPair.CounterSymbol,
                    Values = new ChartValues<double> { },
                    PointGeometry = null
                }
            };

            return chartSeriesCollection;
        }


        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public TimeInterval TimeInterval { get => timeInterval; set => timeInterval = value; }
        public SeriesCollection ChartSeriesCollection
        {
            get
            {
                if (chartSeriesCollection == null)
                {
                    chartSeriesCollection = CreateChartSeriesCollection();
                }

                return chartSeriesCollection;
            }
        }
        public string ChartTitle
        {
            get
            {
                tradingPair ??= GetTradingPairFromSettings();
                return tradingPair.CounterSymbol + " price for 1 " + tradingPair.BaseSymbol; ;
            }
        }

        public Func<double, string> ChartYFormatter
        {
            get
            {
                tradingPair ??= GetTradingPairFromSettings();
                return value => CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + value;
            }
        }

        public ObservableCollection<TradingPair> TradingPairs
        {
            get
            {
                tradingPairs ??= CreateTradingPairs();
                return tradingPairs;
            }
        }

        public ObservableCollection<Market> Markets
        {
            get
            {
                markets ??= CreateMarkets();
                return markets;
            }
        }

        public ObservableCollection<TimeInterval> TimeIntervals
        {
            get
            {
                timeIntervals ??= CreateTimeIntervals();
                return timeIntervals;
            }
        }

        public ObservableCollection<string> ChartLabels
        {
            get
            {
                chartLabels ??= CreateChartLabels();
                return chartLabels;
            }
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
                    var dbFirstTradingPair = db.Markets.Find(market.MarketId).TradingPairs.First();

                    if (dbFirstTradingPair != null)
                    {
                        var settings = db.Configuration.First();
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
                    var settings = db.Configuration.First();
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
                    var settings = db.Configuration.First();
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
                if (await db.TradingPairs.FindAsync(tradingPair.TradingPairId) is TradingPair dbTradingPair)
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
                    //if (values.Count > 500)
                    //{
                    //    int nthSkipValue = values.Count / 500;
                    //    values = values.Where((x, i) => i % nthSkipValue == 0).ToList();
                    //    labels = labels.Where((x, i) => i % nthSkipValue == 0).ToList();
                    //}
                    chartSeriesCollection[0].Values = new ChartValues<double>(values);


                    var labels = chartTicks.Select(tick => tick.Date.ToString(timeFormat)).ToArray();
                    chartLabels.Clear();
                    foreach (var label in labels)
                    {
                        chartLabels.Add(label);
                    }

                    chartYFormatter = value => CurrencyCodeMapper.GetSymbol(dbTradingPair.CounterSymbol) + " " + value;
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
