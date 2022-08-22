using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA.Indicators
{
    internal interface IIndicator
    {
        public string Name { get; }
        public string Description { get; }
        public double Run(List<Tick> ticks);
    }
}
