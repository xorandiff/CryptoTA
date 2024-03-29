﻿using CryptoTA.Database.Models;
using System.Collections.Generic;
using System;

namespace CryptoTA.Indicators
{
    public class ExponentialMovingAverage : IIndicator
    {
        public string Name => "Exponential Moving Average (EMA)";

        public string Description => "";

        public IndicatorResult Run(List<Tick> ticks, Tick currentTick)
        {
            var ticksCount = ticks.Count;

            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < ticksCount; i++)
            {
                var pow = Math.Pow(2d / (i + 2), i);
                nominator += ticks[i].Close * pow;
                denominator += pow;
            }

            var ema = nominator / denominator;

            return new IndicatorResult
            {
                Name = $"{Name} ({ticks.Count})",
                Value = ema,
                ShouldBuy = currentTick.Close != ema ? (ema < currentTick.Close) : null
            };
        }
    }
}
