using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Indicators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace CryptoTA.Pages
{
    public partial class IndicatorsPage : Page
    {
        private DatabaseModel databaseModel;
        private MovingAverages movingAverages = new();
        private Oscillators oscillators = new();
        private readonly ObservableCollection<TimeInterval> timeIntervals = new();
        private TimeInterval timeInterval = new();
        private Tick currentTick = new();

        public Market Market{ get; set; } = new();
        public TradingPair TradingPair { get; set; } = new();
        public TimeInterval TimeInterval { get => timeInterval; set => timeInterval = value; }
        public ObservableCollection<TimeInterval> TimeIntervals { get => timeIntervals; }

        public IndicatorsPage(DatabaseModel dbModel)
        {
            InitializeComponent();
            DataContext = this;

            databaseModel = dbModel;
            databaseModel.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                timeIntervals.Clear();
                foreach (var newTimeInterval in CreateTimeIntervals())
                {
                    timeIntervals.Add(newTimeInterval); 
                }

                Market = await db.GetMarketFromSettings();
                MarketTextBlock.Text = $" ({Market.Name})";

                TradingPair = await db.GetTradingPairFromSettings();

                if (await databaseModel.GetTick(TradingPair) is Tick tick)
                {
                    currentTick = tick;
                    PriceTextBlock.Text = $"{currentTick.Close}/{TradingPair.BaseSymbol}";
                }

                timeInterval = await db.GetTimeIntervalFromSettings(true);
                timeInterval = timeIntervals.Where(ti => ti.TimeIntervalId == timeInterval.TimeIntervalId).First();
                TimeIntervalComboBox.SelectedItem = timeInterval;
            }
        }

        private static ObservableCollection<TimeInterval> CreateTimeIntervals()
        {
            var timeIntervals = new ObservableCollection<TimeInterval>();

            using (var db = new DatabaseContext())
            {
                foreach (var dbTimeInterval in db.TimeIntervals.Where(ti => ti.IsIndicatorInterval).ToList())
                {
                    timeIntervals.Add(dbTimeInterval);
                }
            }

            return timeIntervals;
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is not List<Tick> ticks || TimeInterval.Seconds == 0)
            {
                TimeIntervalComboBox.IsEnabled = true;
                return;
            }

            var movingAveragesResult = movingAverages.Run(ticks, TimeInterval.Seconds, currentTick);
            MovingAveragesItemsControl.ItemsSource = movingAveragesResult;

            var buyCount = movingAveragesResult.Where(ir => ir.ShouldBuy == true).Count();
            var sellCount = movingAveragesResult.Where(ir => ir.ShouldBuy == false).Count();
            var neutralCount = movingAveragesResult.Where(ir => ir.ShouldBuy == null).Count();
            var total = buyCount * 1d + sellCount + neutralCount;

            double movingAveragesCountRatio = (buyCount * 1d + neutralCount * 0.5d) / total;

            if (movingAveragesCountRatio >= 0.8)
            {
                MovingAveragesResultTextBlock.Text = "Strong Buy";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(115, 185, 115));
            }
            else if (movingAveragesCountRatio >= 0.6)
            {
                MovingAveragesResultTextBlock.Text = "Buy";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(159, 200, 159));
            }
            else if (movingAveragesCountRatio >= 0.4)
            {
                MovingAveragesResultTextBlock.Text = "Neutral";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
            else if (movingAveragesCountRatio >= 0.25)
            {
                MovingAveragesResultTextBlock.Text = "Sell";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(222, 168, 168));
            }
            else
            {
                MovingAveragesResultTextBlock.Text = "Strong Sell";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 127, 127));
            }

            MovingAveragesBuyCountTextBlock.Text = buyCount.ToString();
            MovingAveragesSellCountTextBlock.Text = sellCount.ToString();
            MovingAveragesNeutralCountTextBlock.Text = neutralCount.ToString();

            var oscillatorsResult = oscillators.Run(ticks, TimeInterval.Seconds, currentTick);
            OscillatorsItemsControl.ItemsSource = oscillatorsResult;

            var oBuyCount = oscillatorsResult.Where(ir => ir.ShouldBuy == true).Count();
            var oSellCount = oscillatorsResult.Where(ir => ir.ShouldBuy == false).Count();
            var oNeutralCount = oscillatorsResult.Where(ir => ir.ShouldBuy == null).Count();
            var oTotal = oBuyCount * 1d + oSellCount + oNeutralCount;

            double oscillatorsCountRatio = (oBuyCount * 1d + oNeutralCount * 0.5d) / oTotal;

            if (oscillatorsCountRatio >= 0.8)
            {
                OscillatorsResultTextBlock.Text = "Strong Buy";
                OscillatorsResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(115, 185, 115));
            }
            else if (oscillatorsCountRatio >= 0.6)
            {
                OscillatorsResultTextBlock.Text = "Buy";
                OscillatorsResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(159, 200, 159));
            }
            else if (oscillatorsCountRatio >= 0.4)
            {
                OscillatorsResultTextBlock.Text = "Neutral";
                OscillatorsResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            }
            else if (oscillatorsCountRatio >= 0.25)
            {
                OscillatorsResultTextBlock.Text = "Sell";
                OscillatorsResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(222, 168, 168));
            }
            else
            {
                OscillatorsResultTextBlock.Text = "Strong Sell";
                OscillatorsResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 127, 127));
            }

            OscillatorsBuyCountTextBlock.Text = oBuyCount.ToString();
            OscillatorsSellCountTextBlock.Text = oSellCount.ToString();
            OscillatorsNeutralCountTextBlock.Text = oNeutralCount.ToString();

            var tBuy = buyCount + oBuyCount;
            var tSell = sellCount + oSellCount;
            var tNeutral = neutralCount + oNeutralCount;

            double overallCount = total + oTotal;

            double overallRatio = (tBuy * 1d + tNeutral * 0.5d) / overallCount;

            if (overallRatio >= 0.8)
            {
                OverallResultTextBlock.Text = "Strong Buy";
                OverallResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(115, 185, 115));
            }
            else if (overallRatio >= 0.6)
            {
                OverallResultTextBlock.Text = "Buy";
                OverallResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(159, 200, 159));
            }
            else if (overallRatio >= 0.4)
            {
                OverallResultTextBlock.Text = "Neutral";
                OverallResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            }
            else if (overallRatio >= 0.25)
            {
                OverallResultTextBlock.Text = "Sell";
                OverallResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(222, 168, 168));
            }
            else
            {
                OverallResultTextBlock.Text = "Strong Sell";
                OverallResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 127, 127));
            }

            OverallBuyCountTextBlock.Text = tBuy.ToString();
            OverallSellCountTextBlock.Text = tSell.ToString();
            OverallNeutralCountTextBlock.Text = tNeutral.ToString();

            TimeIntervalComboBox.IsEnabled = true;
        }

        private async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is TimeInterval selectedTimeInterval)
            {
                if (MovingAveragesItemsControl != null && !databaseModel.worker.IsBusy)
                {
                    TimeIntervalComboBox.IsEnabled = false;
                    using var db = new DatabaseContext();
                    db.Settings.First().TimeIntervalIdIndicators = selectedTimeInterval.TimeIntervalId;
                    await db.SaveChangesAsync();


                    if (await databaseModel.GetTick(TradingPair) is Tick tick)
                    {
                        currentTick = tick;
                        PriceTextBlock.Text = $"{currentTick.Close}/{TradingPair.BaseSymbol}";
                    }

                    databaseModel.GetTicks(TradingPair.TradingPairId, DateTime.Now.AddSeconds(-200 * selectedTimeInterval.Seconds), 1000);
                }
            }
        }

        public static implicit operator Uri(IndicatorsPage v)
        {
            throw new NotImplementedException();
        }
    }
}
