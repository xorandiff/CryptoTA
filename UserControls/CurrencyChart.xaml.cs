﻿using System;
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
using Telerik.Windows.Controls.FieldList;

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
                
                chartLabels = CreateChartLabels();
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

                    await LoadChartData();
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
                    settings.TimeIntervalIdChart = timeInterval.TimeIntervalId;

                    await db.SaveChangesAsync();
                }

                await LoadChartData(DateTime.Now.AddSeconds(-timeInterval.Seconds));
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
                        var tickData = await marketApis.ActiveMarketApi.GetTick(tradingPair);
                        CurrentPriceText.Text = CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + tickData.Close;
                        CurrentBaseSymbolTextBlock.Text = "/" + tradingPair.BaseSymbol;

                        var dayBefore = tickData.Date.AddDays(-1);
                        var dayBeforeTick = await db.Ticks
                                            .Where(t => t.TradingPairId == TradingPair.TradingPairId && t.Date <= dayBefore)
                                            .OrderByDescending(t => t.Date)
                                            .FirstOrDefaultAsync();

                        if (dayBeforeTick != null)
                        {
                            double percents = (tickData.Close - dayBeforeTick.Close) / dayBeforeTick.Close * 100d;
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
                chartTicks = await db.GetTicks(TradingPair.TradingPairId, startDate, 500);

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
