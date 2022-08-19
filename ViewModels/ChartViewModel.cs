using System.Linq;
using System.Collections.ObjectModel;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Chart;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using CryptoTA.Apis;
using CryptoTA.Utils;

namespace CryptoTA.ViewModels
{
    public class ChartViewModel
    {
        public ChartViewModel()
        {
            using (var db = new DatabaseContext())
            if (!db.Configuration.Any())
            {
                throw new Exception("Configuration data missing.");
            }

            Settings settings;

            using (var db = new DatabaseContext())
            {
                settings = db.Configuration.Include("TradingPair").Include("TimeInterval").First();

                var marketTemp = db.TradingPairs.Find(settings.TradingPair.TradingPairId).Market;
                if (marketTemp != null)
                {
                    market = marketTemp;
                }
                else
                {
                    throw new Exception("Selected trading pair has no corresponding market assigned.");
                }

                tradingPair = settings.TradingPair;
                timeInterval = settings.TimeInterval;
            }


            bool marketApiFound = marketApis.setActiveApiByName(market.Name);
            if (!marketApiFound)
            {
                throw new Exception("No market api found that correspond to database market name.");
            }

            markets = CreateMarkets();
            tradingPairs = CreateTradingPairs();
            timeIntervals = CreateTimeIntervals();

            chartTitle = CreateChartTitle();
            chartLabels = CreateChartLabels();
            chartYFormatter = CreateChartYFormatter();
            chartSeriesCollection = CreateChartSeriesCollection();
        }

        private static ObservableCollection<Market> CreateMarkets()
        {
            ObservableCollection<Market> markets = new();

            using var db = new DatabaseContext();
            var marketList = db.Markets.Include("TradingPairs").ToList();

            foreach (var market in marketList)
            {
                markets.Add(market);
            }

            return markets;
        }

        private ObservableCollection<TradingPair> CreateTradingPairs()
        {
            ObservableCollection<TradingPair> tradingPairs = new();
            if (market != null)
            {
                using (var db = new DatabaseContext())
                {
                    foreach (TradingPair tradingPair in db.Markets.Find(market.MarketId).TradingPairs.ToList())
                    {
                        tradingPairs.Add(tradingPair);
                    }
                }
            }
            return tradingPairs;
        }

        private static ObservableCollection<TimeInterval> CreateTimeIntervals()
        {
            var timeIntervals = new ObservableCollection<TimeInterval>();

            using (var db = new DatabaseContext())
            {
                foreach (var timeInterval in db.TimeIntervals.ToList())
                {
                    timeIntervals.Add(timeInterval);
                }
            }

            return timeIntervals;
        }

        private static ObservableCollection<string> CreateChartLabels()
        {
            ObservableCollection<string> chartLabels = new();
            
            return chartLabels;
        }
        private string CreateChartTitle()
        {
            if (tradingPair == null)
            {
                return "";
            }

            chartTitle = tradingPair.CounterSymbol + " price for 1 " + tradingPair.BaseSymbol;

            return chartTitle;
        }

        private Func<double, string> CreateChartYFormatter()
        {
            if (tradingPair == null)
            {
                return value => value.ToString();
            }

            chartYFormatter = value => CurrencyCodeMapper.GetSymbol(tradingPair.CounterSymbol) + " " + value;

            return chartYFormatter;
        }
        private SeriesCollection CreateChartSeriesCollection()
        {
            if (tradingPair == null)
            {
                return new SeriesCollection();
            }

            chartSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = tradingPair.BaseSymbol + "/" + tradingPair.CounterSymbol,
                    Values = new ChartValues<double> { },
                    PointGeometry = null
                }
            };

            return chartSeriesCollection;
        }


        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public TimeInterval TimeInterval { get => timeInterval; set => timeInterval = value; }
        public SeriesCollection ChartSeriesCollection
        {
            get
            {
                if (chartSeriesCollection == null)
                {
                    chartSeriesCollection = CreateChartSeriesCollection();
                }

                return chartSeriesCollection;
            }
        }
        public string ChartTitle
        {
            get
            {
                if (chartTitle == null)
                {
                    chartTitle = CreateChartTitle();
                }

                return chartTitle;
            }
        }
        public Func<double, string> ChartYFormatter
        {
            get
            {
                if (chartYFormatter == null)
                {
                    chartYFormatter = CreateChartYFormatter();
                }
                return chartYFormatter;
            }
        }

        public ObservableCollection<TradingPair> TradingPairs
        {
            get
            {
                if (tradingPairs == null)
                {
                    tradingPairs = CreateTradingPairs();
                }
                return tradingPairs;
            }
        }

        public ObservableCollection<Market> Markets
        {
            get
            {
                if (markets == null)
                {
                    markets = CreateMarkets();
                }

                return markets;
            }
        }

        public ObservableCollection<TimeInterval> TimeIntervals
        {
            get
            {
                if (timeIntervals == null)
                {
                    timeIntervals = CreateTimeIntervals();
                }
                return timeIntervals;
            }
        }

        public ObservableCollection<string> ChartLabels
        {
            get
            {
                if (chartLabels == null)
                {
                    chartLabels = CreateChartLabels();
                }
                return chartLabels;
            }
        }

        private MarketApis marketApis = new();
        private Market market;
        private TradingPair tradingPair;
        private TimeInterval timeInterval;
        private string chartTitle;
        private Func<double, string> chartYFormatter;

        private ObservableCollection<Market> markets;
        private ObservableCollection<TradingPair> tradingPairs;
        private ObservableCollection<TimeInterval> timeIntervals;
        private ObservableCollection<string> chartLabels;
        private SeriesCollection chartSeriesCollection;

    }
}