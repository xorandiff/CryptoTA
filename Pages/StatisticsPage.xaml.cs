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
using System.Windows.Shapes;

namespace CryptoTA.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        private readonly MarketApis marketApis = new();
        private ObservableCollection<Market> markets = new();
        public ObservableCollection<Market> Markets { get => markets; }
        public Market Market { get; set; }

        public StatisticsPage()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new DatabaseContext())
            {
                foreach (var market in db.Markets.Include(m => m.Credentials).ToList())
                {
                    markets.Add(market);
                }

                var selectedMarketId = db.GetMarketFromSettings().MarketId;
                Market = markets.Where(m => m.MarketId == selectedMarketId).FirstOrDefault()!;
            }
        }

        private void MarketsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Market != null)
            {
                if (Market.Credentials.Any())
                {
                    bool marketApiFound = marketApis.setActiveApiByName(Market.Name);
                    if (!marketApiFound)
                    {
                        throw new Exception("No market API found that correspond to database market name.");
                    }
                }
                else
                {

                }
            }
        }
    }
}
