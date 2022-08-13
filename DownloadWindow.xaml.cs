using CryptoTA.Apis;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Collections.ObjectModel;

namespace CryptoTA
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : NavigationWindow
    {
        public DownloadWindow(bool isDownloadCompleted = false)
        {
            InitializeComponent();
            if (isDownloadCompleted)
            {
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Navigate(new DownloadPage1());
        }
    }
}
