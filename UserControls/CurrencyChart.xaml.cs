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

                Settings settings;

                using (var db = new DatabaseContext())
                {
                    settings = db.Configuration.Include("TradingPair").Include("TimeInterval").First();

                    TradingPair? dbTradingPair = db.TradingPairs.Find(settings.TradingPair.TradingPairId);
                    if (dbTradingPair != null)
                    {
                        if (dbTradingPair.Market != null)
                        {
                            market = dbTradingPair.Market;
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

                    tradingPair = settings.TradingPair;
                    timeInterval = settings.TimeInterval;
                }

                bool marketApiFound = marketApis.setActiveApiByName(market.Name);
                if (!marketApiFound)
                {
                    throw new Exception("No market api found that correspond to database market name.");
                }

                markets = CreateMarkets();
                tradingPairs = CreateTradingPairs();
                timeIntervals = CreateTimeIntervals();

                chartTitle = CreateChartTitle();
                chartLabels = CreateChartLabels();
                chartYFormatter = CreateChartYFormatter();
                chartSeriesCollection = CreateChartSeriesCollection();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static ObservableCollection<Market> CreateMarkets()
        {
            ObservableCollection<Market> markets = new();

            using var db = new DatabaseContext();
            var marketList = db.Markets.Include("TradingPairs").ToList();

            foreach (var market in marketList)
            {
                markets.Add(market);
            }

            return markets;
        }

        private ObservableCollection<TradingPair> CreateTradingPairs()
        {
            ObservableCollection<TradingPair> tradingPairs = new();
            if (market != null)
            {
                using (var db = new DatabaseContext())
                {
                    foreach (TradingPair tradingPair in db.Markets.Find(market.MarketId).TradingPairs.ToList())
                    {
                        tradingPairs.Add(tradingPair);
                    }
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
        private string CreateChartTitle()
        {
            if (tradingPair == null)
            {
                return "";
            }

            chartTitle = tradingPair.CounterSymbol + " price for 1 " + tradingPair.BaseSymbol;

            return chartTitle;
        }

        private Func<double, string> CreateChartYFormatter()
        {
            if (tradingPair == null)
            {
                return value => value.ToString();
            }

            chartYFormatter = value => CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + value;

            return chartYFormatter;
        }
        private SeriesCollection CreateChartSeriesCollection()
        {
            if (tradingPair == null)
            {
                return new SeriesCollection();
            }

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
                if (chartTitle == null)
                {
                    chartTitle = CreateChartTitle();
                }

                return chartTitle;
            }
        }
        public Func<double, string> ChartYFormatter
        {
            get
            {
                if (chartYFormatter == null)
                {
                    chartYFormatter = CreateChartYFormatter();
                }
                return chartYFormatter;
            }
        }

        public ObservableCollection<TradingPair> TradingPairs
        {
            get
            {
                if (tradingPairs == null)
                {
                    tradingPairs = CreateTradingPairs();
                }
                return tradingPairs;
            }
        }

        public ObservableCollection<Market> Markets
        {
            get
            {
                if (markets == null)
                {
                    markets = CreateMarkets();
                }

                return markets;
            }
        }

        public ObservableCollection<TimeInterval> TimeIntervals
        {
            get
            {
                if (timeIntervals == null)
                {
                    timeIntervals = CreateTimeIntervals();
                }
                return timeIntervals;
            }
        }

        public ObservableCollection<string> ChartLabels
        {
            get
            {
                if (chartLabels == null)
                {
                    chartLabels = CreateChartLabels();
                }
                return chartLabels;
            }
        }

        private MarketApis marketApis = new();
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
                TradingPairComboBox.IsEnabled = false;

                using (var db = new DatabaseContext())
                {
                    var settings = db.Configuration.First();
                    settings.TradingPairId = tradingPair.TradingPairId;

                    db.SaveChanges();
                }
                await LoadChartData();

                TradingPairComboBox.IsEnabled = true;
            }
        }

        private async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is TimeInterval timeInterval)
            {
                TimeIntervalComboBox.IsEnabled = false;

                using (var db = new DatabaseContext())
                {
                    var settings = db.Configuration.First();
                    settings.TimeIntervalId = timeInterval.TimeIntervalId;

                    db.SaveChanges();
                }
                await LoadChartData(DateTime.Now.AddSeconds(-1 * timeInterval.Seconds));

                TimeIntervalComboBox.IsEnabled = true;
            }
        }

        private async Task LoadChartData(DateTime? startDate = null)
        {
            if (TradingPairComboBox.SelectedItem is not TradingPair tradingPair)
            {
                return;
            }

            using (DatabaseContext db = new())
            {
                if (db.TradingPairs.Find(tradingPair.TradingPairId) is TradingPair dbTradingPair)
                {
                    if (!dbTradingPair.Ticks.Any())
                    {
                        var ticks = await marketApis.ActiveMarketApi.GetOhlcData(tradingPair, null, marketApis.ActiveMarketApi.OhlcTimeIntervals.Min());

                        db.TradingPairs.Find(tradingPair.TradingPairId).Ticks.AddRange(ticks);

                        db.SaveChanges();
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
                    chartTicks = tradingPair.Ticks.Where(tick => tick.Date >= startDate).ToList();

                    var values = chartTicks.Select(tick => (object)tick.Close).ToList();
                    var labels = chartTicks.Select(tick => tick.Date.ToString(timeFormat));

                    if (values.Count > 500)
                    {
                        int nthSkipValue = values.Count / 500;
                        values = values.Where((x, i) => i % nthSkipValue == 0).ToList();
                        labels = labels.Where((x, i) => i % nthSkipValue == 0).ToList();
                    }

                    chartSeriesCollection[0].Values.Clear();
                    chartSeriesCollection[0].Values.AddRange(values);
                }
            }
        }
    }
}
