using System.Collections.ObjectModel;

namespace CryptoTA.Chart
{
    public class ChartTimeSpans : ObservableCollection<ChartTimeSpan>
    {
        public ChartTimeSpans() : base()
        {
            Add(new ChartTimeSpan("1 day", 60 * 60 * 24));
            Add(new ChartTimeSpan("3 days", 60 * 60 * 24 * 3));
            Add(new ChartTimeSpan("1 week", 60 * 60 * 24 * 7));
            Add(new ChartTimeSpan("2 weeks", 60 * 60 * 24 * 14));
            Add(new ChartTimeSpan("1 month", 60 * 60 * 24 * 31));
            Add(new ChartTimeSpan("3 months", 60 * 60 * 24 * 31 * 3));
            Add(new ChartTimeSpan("6 months", 60 * 60 * 24 * 31 * 6));
            Add(new ChartTimeSpan("1 year", 60 * 60 * 24 * 31 * 12));
            Add(new ChartTimeSpan("5 years", 60 * 60 * 24 * 31 * 12 * 5));
        }
    }
}
