using CryptoTA.Apis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CryptoTA
{
    public partial class DownloadPage1 : Page
    {
        public DownloadPage1()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var marketApis = Resources["MarketApisData"] as MarketApis;
            if (marketApis != null)
            {
                NavigationService.Navigate(new DownloadPage2(marketApis));
            }
        }
    }
}
