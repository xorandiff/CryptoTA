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
        private readonly ObservableCollection<TimeInterval> timeIntervals = new();
        private TimeInterval timeInterval = new();

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
                MarketTextBlock.Text = Market.Name;

                TradingPair = await db.GetTradingPairFromSettings();
                TradingPairTextBlock.Text = TradingPair.DisplayName;

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
            if (e.Result is not List<Tick> ticks)
            {
                TimeIntervalComboBox.IsEnabled = true;
                return;
            }

            var movingAveragesResult = movingAverages.Run(ticks, TimeInterval.Seconds);
            MovingAveragesItemsControl.ItemsSource = movingAveragesResult;

            var movingAveragesBuyCount = movingAveragesResult.Where(ir => ir.ShouldBuy).Count();
            var movingAveragesSellCount = movingAveragesResult.Where(ir => !ir.ShouldBuy).Count();
            double movingAveragesCountRatio = movingAveragesBuyCount / (movingAveragesBuyCount + movingAveragesSellCount);

            if (movingAveragesCountRatio >= 0.8)
            {
                MovingAveragesResultTextBlock.Text = "Strong Buy";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
            else if (movingAveragesCountRatio >= 0.6)
            {
                MovingAveragesResultTextBlock.Text = "Buy";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 155, 0));
            }
            else if (movingAveragesCountRatio >= 0.4)
            {
                MovingAveragesResultTextBlock.Text = "Neutral";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
            else if (movingAveragesCountRatio >= 0.25)
            {
                MovingAveragesResultTextBlock.Text = "Sell";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(155, 0, 0));
            }
            else
            {
                MovingAveragesResultTextBlock.Text = "Strong Sell";
                MovingAveragesResultTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }

            MovingAveragesBuyCountTextBlock.Text = movingAveragesBuyCount.ToString();
            MovingAveragesSellCountTextBlock.Text = movingAveragesSellCount.ToString();

            TimeIntervalComboBox.IsEnabled = true;
        }

        private async void TimeIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeIntervalComboBox.SelectedItem is TimeInterval selectedTimeInterval)
            {
                if (MovingAveragesItemsControl != null && !databaseModel.worker.IsBusy)
                {
                    TimeIntervalComboBox.IsEnabled = false;
                    using (var db = new DatabaseContext())
                    {
                        db.Settings.First().TimeIntervalIdIndicators = selectedTimeInterval.TimeIntervalId;
                        await db.SaveChangesAsync();

                        databaseModel.GetTicks(TradingPair.TradingPairId, DateTime.Now.AddSeconds(-200 * selectedTimeInterval.Seconds), 1000);
                    }
                }
            }
        }

        public static implicit operator Uri(IndicatorsPage v)
        {
            throw new NotImplementedException();
        }
    }
}
