using System.Linq;
using System.Windows;
using CryptoTA.Database;
using CryptoTA.UserControls;
using CryptoTA.Pages;

namespace CryptoTA
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseModel databaseModel;

        // Controls
        private readonly StatusBarControl statusBarControl;
        private readonly CurrencyChart currencyChart;

        // Pages
        private readonly IndicatorsPage indicatorsPage;
        private readonly StatisticsPage statisticsPage;
        private readonly StrategySettingsPage strategySettingsPage;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            databaseModel = new();

            statusBarControl = new();
            currencyChart = new(databaseModel);

            indicatorsPage = new(databaseModel);
            statisticsPage = new();
            strategySettingsPage = new();

            // Subscribe StatusBar to BackgroundWorker events before loading
            databaseModel.worker.ProgressChanged += statusBarControl.Worker_ProgressChanged;
            databaseModel.worker.RunWorkerCompleted += statusBarControl.Worker_RunWorkerCompleted;

            ChartGrid.Children.Add(currencyChart);
            BottomStackPanel.Children.Add(statusBarControl);

            IndicatorsPageFrame.Content = indicatorsPage;
            StatisticsPageFrame.Content = statisticsPage;
            StrategySettingsPageFrame.Content = strategySettingsPage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using var db = new DatabaseContext();
            if (!db.Settings.Any())
            {
                _ = new DownloadWindow().ShowDialog();
            }
        }

        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _ = new AccountsWindow { Owner = this }.ShowDialog();
        }
    }
}
