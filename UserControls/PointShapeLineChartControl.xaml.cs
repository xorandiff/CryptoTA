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
                    PointGeometry = null
                }
            };

            Labels = new List<string>();
            YFormatter = value => value.ToString("C", CultureInfo.CreateSpecificCulture(_cultureString));
            ChartTitle = _realCurrency + " price for 1 " + _cryptoCurrency;

            DataContext = this;
        }
        private string _cultureString = "en-us";
        private string _realCurrency = "USD";
        private string _cryptoCurrency = "ETH";
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public string ChartTitle { get; set; }

        public void changeByRate(double rate, string currencySymbol, string currencyCulture)
        {
            var newValues = new ChartValues<double> { };

            foreach (var value in SeriesCollection[0].Values)
            {
                newValues.Add(rate * (double)value);
            }

            var newCurrencySeries = new LineSeries
            {
                Title = "ETH/" + currencySymbol,
                Values = newValues,
                PointGeometry = null
            };

            _cultureString = currencyCulture;
            _realCurrency = currencySymbol;
            ChartTitle = currencySymbol + " price for 1 " + _cryptoCurrency;

            SeriesCollection.Clear();
            SeriesCollection.Add(newCurrencySeries);
        }
    }
}
