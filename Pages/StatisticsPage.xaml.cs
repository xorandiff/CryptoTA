using CryptoTA.Apis;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using Microsoft.EntityFrameworkCore;
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

namespace CryptoTA.Pages
{
    public partial class StatisticsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private ObservableCollection<Market> markets = new();
        public ObservableCollection<Market> Markets { get; set; } = new();
        public Market Market { get; set; } = new();

        public StatisticsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var market in db.Markets.Include(m => m.Credentials).ToList())
                {
                    markets.Add(market);
                }

                var selectedMarketId = (await db.GetMarketFromSettings()).MarketId;
                Market = markets.Where(m => m.MarketId == selectedMarketId).FirstOrDefault()!;
            }
        }

        private async void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
                    if (!marketApis.setActiveApiByName(Market.Name))
                    {
                        throw new Exception("No market API found that correspond to database market name.");
                    }

                    //AccountBalanceItemsControl.ItemsSource = await marketApis.ActiveMarketApi.GetAccountBalance();
                }
                else
                {

                }
            }
        }
    }
}
