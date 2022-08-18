using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Indicators
{
    public class SimpleMovingAverage : IIndicator
    {
        public string Name => "Simple Moving Average (SMA)";

        public string Description => "";

        public double Run(List<Tick> ticks)
        {
            double result = 0;

            foreach (var tick in ticks)
            {
                result += tick.Close;
            }

            return result / ticks.Count;
        }
    }
}
