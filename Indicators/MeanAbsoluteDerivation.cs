using CryptoTA.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTA.Indicators
{
    public class MeanAbsoluteDerivation
    {
        public string Name => "Mean Absolute Derivation (MD)";

        public string Description => "";

        public double Run(List<double> values)
        {
            double result = 0;
            double centralPoint = values.Last();

            foreach (var value in values)
            {
                result += Math.Abs(value - centralPoint);
            }

            return result / values.Count * 1d;
        }
    }
}
