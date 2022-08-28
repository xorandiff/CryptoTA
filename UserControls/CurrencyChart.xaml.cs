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
using System.Threading;
using System.Windows.Media;
using System.ComponentModel;

namespace CryptoTA.UserControls
{
    public partial class CurrencyChart : UserControl
    {
        private readonly MarketApis marketApis = new();
        private Func<double, string> yAxisLabeler = value => value.ToString();
        private DateTime? startDate = null;
        private DatabaseModel databaseModel;

        private ObservableCollection<Market> markets = new();
        private ObservableCollection<TradingPair> tradingPairs = new();
        private ObservableCollection<TimeInterval> timeIntervals = new();

        private readonly ObservableCollection<string> observableLabels = new();
        private readonly ObservableCollection<Tick> observableValues = new();

        public CurrencyChart(DatabaseModel dbModel)
        {
            InitializeComponent();
            DataContext = this;

            databaseModel = dbModel;
            databaseModel.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            try
            {
                using (var db = new DatabaseContext())
                if (!db.Settings.Any())
                {
                    throw new Exception("Configuration data missing.");
                }

                XAxes = new ObservableCollection<Axis>
                {
                    new Axis
                    {
                        Name = "Time",
                        Labels = observableLabels,
                        MaxLimit = 500,
                    }
                };

                YAxes = new ObservableCollection<Axis>
                {
                    new Axis
                    {
                        Name = "Price",
                        Labeler = yAxisLabeler
                    }
                };

                ChartSeriesCollection = new ObservableCollection<ISeries>
                {
                    new LineSeries<Tick>
                    {
                        Values = observableValues,
                        GeometryFill = null,
                        GeometryStroke = null,
                        GeometrySize = 0,
                        Stroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 1 },
                        LineSmoothness = 0,
                        Mapping = (tick, chartPoint) =>
                        {
                            chartPoint.PrimaryValue = tick.Close;
                            chartPoint.SecondaryValue = chartPoint.Context.Entity.EntityIndex;
                        },
                        TooltipLabelFormatter = chartPoint => $"{CurrencyCodeMapper.GetSymbol(TradingPair.CounterSymbol)}{chartPoint.PrimaryValue}\n{observableValues[(int)chartPoint.SecondaryValue].Date:dd.MM.yyyy HH:mm}"
                    }
                };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                markets.Clear();
                foreach (var market in CreateMarkets())
                {
                    markets.Add(market);
                }
                Market = await db.GetMarketFromSettings();
                Market = markets.Where(m => m.MarketId == Market.MarketId).First();
                MarketComboBox.SelectedItem = Market;

                if (!marketApis.setActiveApiByName(Market.Name))
                {
                    throw new Exception("No market API found that correspond to database market name.");
                }

                tradingPairs.Clear();
                foreach (var tradingPair in CreateTradingPairs())
                {
                    tradingPairs.Add(tradingPair);
                }
                TradingPair = await db.GetTradingPairFromSettings();
                TradingPair = tradingPairs.Where(tp => tp.TradingPairId == TradingPair.TradingPairId).First();

                timeIntervals.Clear();
                foreach (var timeInterval in CreateTimeIntervals())
                {
                    timeIntervals.Add(timeInterval);
                }
                TimeInterval = await db.GetTimeIntervalFromSettings(false);
                TimeInterval = timeIntervals.Where(ti => ti.TimeIntervalId == TimeInterval.TimeIntervalId).First();

                await FetchTickData();
            }
        }

        public Market Market { get; set; } = new();
        public TradingPair TradingPair { get; set; } = new();
        public TimeInterval TimeInterval { get; set; } = new();
        public ObservableCollection<ISeries> ChartSeriesCollection { get; set; }
        public ObservableCollection<Axis> XAxes { get; set; }
        public ObservableCollection<Axis> YAxes { get; set; }
        public ObservableCollection<TradingPair> TradingPairs { get => tradingPairs; }
        public ObservableCollection<Market> Markets { get => markets; }
        public ObservableCollection<TimeInterval> TimeIntervals { get => timeIntervals; }

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
                foreach (var timeInterval in db.TimeIntervals.Where(ti => !ti.IsIndicatorInterval).ToList())
                {
                    timeIntervals.Add(timeInterval);
                }
            }

            return timeIntervals;
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


        private async void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is Market market)
            {
                try
                {
                    marketApis.setActiveApiByName(market.Name);

                    using var db = new DatabaseContext();
                    if (!db.TradingPairs.Where(tp => tp.MarketId == market.MarketId).Any())
                    {
                        var newTradingPairs = await marketApis.ActiveMarketApi.GetTradingPairs();
                        if (newTradingPairs == null)
                        {
                            throw new Exception("Couldn't download any trading pair for selected market API. This may be problem with market implementation.");
                        }

                        var dbMarket = await db.Markets.Where(m => m.Name == market.Name).Include(m => m.TradingPairs).FirstOrDefaultAsync();
                        dbMarket!.TradingPairs.AddRange(newTradingPairs);

                        await db.SaveChangesAsync();
                    }

                    tradingPairs.Clear();
                    foreach (var tp in CreateTradingPairs())
                    {
                        tradingPairs.Add(tp);
                    }
                    TradingPair = await db.GetTradingPairFromSettings();
                    if (TradingPair.MarketId != market.MarketId)
                    {
                        TradingPair = tradingPairs[0];
                    }
                    TradingPair = tradingPairs.Where(tp => tp.TradingPairId == TradingPair.TradingPairId).First();
                    TradingPairComboBox.SelectedItem = TradingPair;

                    TimeInterval = await db.GetTimeIntervalFromSettings(false);
                    TimeInterval = timeIntervals.Where(tp => tp.TimeIntervalId == TimeInterval.TimeIntervalId).First();
                    TimeIntervalComboBox.SelectedItem = TimeInterval;

                    LoadChartData();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message);
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

                    await db.SaveChangesAsync();
                }

                LoadChartData();
            }
        }

        private async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is TimeInterval timeInterval)
            {
                using (var db = new DatabaseContext())
                {
                    var settings = db.Settings.First();
                    settings.TimeIntervalIdChart = timeInterval.TimeIntervalId;

                    await db.SaveChangesAsync();
                }

                startDate = DateTime.Now.AddSeconds(-timeInterval.Seconds);
                LoadChartData();
            }
        }

        public async Task FetchTickData()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                using (var db = new DatabaseContext())
                {
                    if (TradingPairComboBox.SelectedItem is TradingPair tradingPair)
                    {
                        if (await marketApis.ActiveMarketApi.GetTick(tradingPair) is not Tick tick)
                        {
                            continue;
                        }

                        CurrentPriceText.Text = CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + tick.Close;
                        CurrentBaseSymbolTextBlock.Text = "/" + tradingPair.BaseSymbol;

                        tick.TradingPairId = TradingPair.TradingPairId;
                        await db.Ticks.AddAsync(tick);

                        await db.SaveChangesAsync();

                        if (TimeInterval.Seconds <= 3600 * 24)
                        {
                            if (observableValues.Any())
                            {
                                observableValues.RemoveAt(0);
                            }
                            observableValues.Add(tick);

                            if (observableLabels.Any())
                            {
                                observableLabels.RemoveAt(0);
                            }
                            observableLabels.Add(tick.Date.ToString("HH:mm"));
                        }

                        var dayBefore = tick.Date.AddDays(-1);
                        var dayBeforeTick = await db.Ticks
                                            .Where(t => t.TradingPairId == TradingPair.TradingPairId && t.Date <= dayBefore)
                                            .OrderByDescending(t => t.Date)
                                            .FirstOrDefaultAsync();

                        if (dayBeforeTick != null)
                        {
                            double percents = (tick.Close - dayBeforeTick.Close) / dayBeforeTick.Close * 100d;
                            string percentString = string.Format("{0:N2}", percents) + "%";

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
                            }
                            else if (percents < 0)
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
                            CurrentChangeText.Foreground = new SolidColorBrush(percentColor);

                            if (percents > 0)
                            {
                                percentString = "+" + percentString;
                            }
                            CurrentChangeText.Text = percentString;
                        }
                    }
                }
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is not List<Tick> chartTicks)
            {
                EnableFilterComboBoxes();
                return;
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

            var values = chartTicks.Select(tick => tick.Close).ToArray();
            var labels = chartTicks.Select(tick => tick.Date.ToString(timeFormat)).ToArray();


            observableValues.Clear();
            foreach (var tick in chartTicks.ToArray())
            {
                observableValues.Add(tick);
            }

            observableLabels.Clear();
            foreach (var label in labels)
            {
                observableLabels.Add(label);
            }

            YAxes[0].Labeler = value => CurrencyCodeMapper.GetSymbol(TradingPair.CounterSymbol) + value.ToString(".##");

            EnableFilterComboBoxes();
        }

        private void LoadChartData()
        {
            DisableFilterComboBoxes();
            databaseModel.GetTicks(TradingPair.TradingPairId, startDate, 500);
        }
    }
}
