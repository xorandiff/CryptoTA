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
