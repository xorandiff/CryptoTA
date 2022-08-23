using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Indicators;
using CryptoTA.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoTA.Pages
{
    public partial class IndicatorsPage : Page
    {
        private MovingAverages movingAverages = new();

        public TradingPair TradingPair { get; set; }
        public ObservableCollection<TimeInterval> Intervals
        {
            get
            {
                ObservableCollection<TimeInterval> result = new()
                {
                    new TimeInterval { Name = "1 minute", Seconds = 60 },
                    new TimeInterval { Name = "5 minutes", Seconds = 60 * 5 },
                    new TimeInterval { Name = "15 minutes", Seconds = 60 * 15 },
                    new TimeInterval { Name = "30 minutes", Seconds = 60 * 30 },
                    new TimeInterval { Name = "1 hour", Seconds = 60 * 60 },
                    new TimeInterval { Name = "2 hours", Seconds = 60 * 60 * 2 },
                    new TimeInterval { Name = "4 hours", Seconds = 60 * 60 * 4 },
                    new TimeInterval { Name = "1 day", Seconds = 60 * 60 * 24 },
                    new TimeInterval { Name = "1 week", Seconds = 60 * 60 * 24 * 7 },
                    new TimeInterval { Name = "1 month", Seconds = 60 * 60 * 24 * 31 },
                };

                return result;
            }
        }

        public IndicatorsPage()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new DatabaseContext())
            {
                TradingPair = db.GetTradingPairFromSettings();
            }
        }

        private async void IntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IntervalComboBox.SelectedItem is TimeInterval timeInterval)
            {
                if (MovingAveragesItemsControl != null)
                {
                    using (var db = new DatabaseContext())
                    {
                        var ticks = await db.GetTicks(TradingPair.TradingPairId, DateTime.Now.AddSeconds(-200 * timeInterval.Seconds));
                        MovingAveragesItemsControl.ItemsSource = movingAverages.Run(ticks, timeInterval.Seconds);
                    }
                }
            }
        }
    }
}
