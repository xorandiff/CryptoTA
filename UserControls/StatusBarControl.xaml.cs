using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CryptoTA.UserControls
{
    public partial class StatusBarControl : UserControl
    {
        public StatusBarControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (!ProgressBarControl.IsVisible)
            {
                ProgressBarControl.Visibility = Visibility.Visible;
                StatusTextBlock.Text = "Downloading OHLC data...";
            }
            ProggressTextBlock.Text = e.ProgressPercentage.ToString() + "%";
            ProgressBarControl.Value = e.ProgressPercentage;
        }

        public void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBarControl.Visibility = Visibility.Hidden;
            StatusTextBlock.Text = "";
            ProggressTextBlock.Text = "";
        }
    }
}
