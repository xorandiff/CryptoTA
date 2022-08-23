using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Indicators
{
    public class IndicatorResult
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public bool ShouldBuy { get; set; }
    }
}
