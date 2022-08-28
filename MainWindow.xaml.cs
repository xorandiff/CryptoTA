using RestSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CryptoTA.Database;
using CryptoTA.UserControls;
using CryptoTA.Pages;

namespace CryptoTA
{
    public partial class MainWindow : Window
    {
        public class ExchangeRateConvertResponse
        {
            public bool Success { get; set; }
            public bool Historical { get; set; }
            public double Result { get; set; }
            public DateTime Date { get; set; }
        }

        private DatabaseModel databaseModel;
        private readonly StatusBarControl statusBarControl;
        private readonly CurrencyChart currencyChart;
        private readonly IndicatorsPage indicatorsPage;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            databaseModel = new();

            statusBarControl = new();
            currencyChart = new(databaseModel);

            indicatorsPage = new(databaseModel);

            databaseModel.worker.ProgressChanged += statusBarControl.Worker_ProgressChanged;
            databaseModel.worker.RunWorkerCompleted += statusBarControl.Worker_RunWorkerCompleted;

            ChartGrid.Children.Add(currencyChart);
            BottomStackPanel.Children.Add(statusBarControl);

            IndicatorsPageFrame.Content = indicatorsPage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using var db = new DatabaseContext();
            if (!db.Settings.Any())
            {
                var downloadWindow = new DownloadWindow();
                downloadWindow.ShowDialog();
            }
        }

        private async Task<double> GetCurrencyRate(string sourceCurrency, string targetCurrency)
        {
            string uriString = "https://api.exchangerate.host/convert?from=" + sourceCurrency + "&to=" + targetCurrency;
            Uri baseUrl = new (uriString);
            RestClient client = new (baseUrl);
            RestRequest request = new (baseUrl, Method.Get);

            var response = await client.ExecuteAsync<ExchangeRateConvertResponse>(request);
            if (response.IsSuccessful && response.Data != null && response.Data.Success)
            {
                return response.Data.Result;
            }

            return 0d;
        }

        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var accountsWindow = new AccountsWindow
            {
                Owner = this
            };
            accountsWindow.ShowDialog();
        }
    }
}
