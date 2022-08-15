using System.Linq;
using System.Collections.ObjectModel;
using CryptoTA.Database;
using CryptoTA.Database.Models;
using CryptoTA.Chart;

namespace CryptoTA.ViewModels
{
    public class ChartViewModel
    {
        public ChartViewModel()
        {
            markets = CreateMarkets();
            market = markets.First();

            tradingPairs = CreateTradingPairs();
            tradingPair = tradingPairs.First();

            timeIntervals = CreateTimeIntervals();
            timeInterval = timeIntervals.First();
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
                foreach (TradingPair tradingPair in market.TradingPairs.ToList())
                {
                    tradingPairs.Add(tradingPair);
                }
            }
            return tradingPairs;
        }

        private static ObservableCollection<ChartTimeSpan> CreateTimeIntervals()
        {
            ObservableCollection<ChartTimeSpan> timeIntervals = new()
            {
                new ChartTimeSpan("1 day", 86400),
                new ChartTimeSpan("3 days", 86400 * 3),
                new ChartTimeSpan("1 week", 86400 * 7),
                new ChartTimeSpan("2 weeks", 86400 * 14),
                new ChartTimeSpan("1 month", 86400 * 31),
                new ChartTimeSpan("3 months", 86400 * 31 * 3),
                new ChartTimeSpan("6 months", 86400 * 31 * 6),
                new ChartTimeSpan("1 year", 86400 * 31 * 12),
                new ChartTimeSpan("5 years", 86400 * 31 * 12 * 5)
            };
            return timeIntervals;
        }

        public Market Market { get => market; set => market = value; }
        public TradingPair TradingPair { get => tradingPair; set => tradingPair = value; }
        public ChartTimeSpan TimeInterval { get => timeInterval; set => timeInterval = value; }

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

        public ObservableCollection<ChartTimeSpan> TimeIntervals
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

        private Market market;
        private TradingPair tradingPair;
        private ChartTimeSpan timeInterval;

        private ObservableCollection<Market> markets;
        private ObservableCollection<TradingPair> tradingPairs;
        private ObservableCollection<ChartTimeSpan> timeIntervals;
    }
}