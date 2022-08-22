using System.Windows;
using System.Windows.Navigation;

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
