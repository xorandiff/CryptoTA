using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
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
using Telerik.Windows.Controls;

namespace CryptoTA
{
    /// <summary>
    /// Logika interakcji dla klasy AccountsWindow.xaml
    /// </summary>
    public partial class AccountsWindow : Window
    {
        private readonly MarketApis marketApis = new();
        public MarketApis MarketApis { get => marketApis; }

        public AccountsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void AccountsSaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MarketApiComboBox.SelectedItem is IMarketApi marketApi && EnabledCheckBox.IsChecked != null && CredentialsRequiredCheckBox.IsChecked != null)
                {
                    using (var db = new DatabaseContext())
                    {
                        if ((bool) EnabledCheckBox.IsChecked)
                        {
                            var market = db.Markets.Include(market => market.Credentials).Where(market => market.Name == marketApi.Name).FirstOrDefault();
                            if (market != null)
                            {
                                market.CredentialsRequired = (bool) CredentialsRequiredCheckBox.IsChecked;

                                if (!market.Credentials.Any())
                                {
                                    market.Credentials.Add(new Credentials { PublicKey = ApiKeyTextBox.Text, PrivateKey = PrivateKeyTextBox.Text });
                                }
                                else
                                {
                                    var credentials = market.Credentials.FirstOrDefault();
                                    if (credentials != null)
                                    {
                                        credentials.PublicKey = ApiKeyTextBox.Text;
                                        credentials.PrivateKey = PrivateKeyTextBox.Text;
                                    }
                                    else
                                    {
                                        throw new Exception("Cannot load selected market API credentials from database.");
                                    }
                                }

                                db.SaveChanges();
                                Close();
                            }
                            else
                            {
                                throw new Exception("Cannot find enabled market API in database.");
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Cannot load one of form's item.");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void MarketApiComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketApiComboBox.SelectedItem is IMarketApi marketApi)
            {
                using (var db = new DatabaseContext())
                {
                    EnabledCheckBox.IsChecked = db.Markets.Where(market => market.Name == marketApi.Name).Any();
                    if ((bool) EnabledCheckBox.IsChecked)
                    {
                        var market = db.Markets.Include(market => market.Credentials).Where(market => market.Name == marketApi.Name).FirstOrDefault();
                        if (market != null)
                        {
                            CredentialsRequiredCheckBox.IsChecked = market.CredentialsRequired;
                            if (market.Credentials.Any())
                            {
                                var credentials = market.Credentials.FirstOrDefault();
                                if (credentials != null)
                                {
                                    ApiKeyTextBox.Text = credentials.PublicKey;
                                    PrivateKeyTextBox.Text = credentials.PrivateKey;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
