using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace CryptoTA.UserControls
{
    /// <summary>
    /// Logika interakcji dla klasy PointShapeLineChart.xaml
    /// </summary>
    public partial class PointShapeLineChartControl : UserControl
    {
        public PointShapeLineChartControl()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "ETH/USD",
                    Values = new ChartValues<double> { },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new List<string>();
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture("en-us"));

            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
    }
}
