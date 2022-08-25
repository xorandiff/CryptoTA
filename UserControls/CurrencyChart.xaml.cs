using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CryptoTA.Apis;
using CryptoTA.Database.Models;
using CryptoTA.Database;
using CryptoTA.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

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
        public ISeries[] ChartSeriesCollection { get; set; } =
        {
            new LineSeries<double>
            {
                Values = Array.Empty<double>(),
                GeometryFill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 1 },
                LineSmoothness = 0
            }
        };
        public ObservableCollection<Axis> XAxes = new()
        {
            new Axis
            {
                Name = "Time",
                Labels = Array.Empty<string>()
            }
        };
        public ObservableCollection<Axis> YAxes = new()
        {
            new Axis
            {
                Name = "Price",
                Labels = Array.Empty<string>()
            }
        };
        public Func<double, string> ChartYFormatter { get => chartYFormatter; }
        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }
        public ObservableCollection<Market> Markets { get => markets; }
        public ObservableCollection<TimeInterval> TimeIntervals { get => timeIntervals; }
        public ObservableCollection<string> ChartLabels { get => chartLabels; }
        public List<Tick> ChartTicks { get => chartTicks; }

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

                using var db = new DatabaseContext();
                chartTicks = await db.GetTicks(TradingPair.TradingPairId, startDate, 1000);

                var values = chartTicks.Select(tick => tick.Close).ToArray();
                var labels = chartTicks.Select(tick => tick.Date.ToString(timeFormat)).ToArray();

                ChartSeriesCollection[0].Values = values;
                ChartSeriesCollection[0].Name = TradingPair.CounterSymbol + " price for 1 " + TradingPair.BaseSymbol;

                XAxes = new ObservableCollection<Axis>
                {
                    new Axis { Name = "Time", Labels = labels }
                };
                chartYFormatter = value => CurrencyCodeMapper.GetSymbol(TradingPair.CounterSymbol) + " " + value;
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
