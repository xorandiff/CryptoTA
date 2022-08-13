
namespace CryptoTA.Chart
{
    public class ChartTimeSpan
    {
        public string Name { get; set; }
        public uint Value { get; set; }

        public ChartTimeSpan(string name, uint value)
        {
            Name = name;
            Value = value;
        }
    }
}
