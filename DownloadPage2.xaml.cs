using CryptoTA.Apis;
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

namespace CryptoTA
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadPage1.xaml
    /// </summary>
    public partial class DownloadPage2 : Page
    {
        private MarketApis marketApis;
        public DownloadPage2(MarketApis marketApisCollection)
        {
            InitializeComponent();
            marketApis = marketApisCollection;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //this.NavigationService.Navigate()
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int totalTradingPairsCount = 0;

                foreach (var marketApi in marketApis)
                {

                    if (marketApi.Enabled)
                    {
                        var tradingPairs = await marketApi.GetTradingPairs();
                        totalTradingPairsCount += tradingPairs.Length;
                    }
                }

                DoneLabel.Content = $"Done. Total trading pairs downloaded: {totalTradingPairsCount}";
                SaveButton.IsEnabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
