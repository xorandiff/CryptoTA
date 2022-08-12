using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTA
{

    class TradingPair
    {
        public string? Name { get; set; }
        public string? AlternativeName { get; set; }
        public string? WebsocketName { get; set; }
        public string? BaseName { get; set; }
        public string? CounterName { get; set; }
        public string? BaseSymbol { get; set; }
        public string? CounterSymbol { get; set; }

        public uint BaseDecimals { get; set; }
        public uint CounterDecimals { get; set; }
        public double MinimalOrderAmount { get; set; }
    }

    class Fees
    {
        public double MakerFee { get; set; }
        public double TakerFee { get; set; }
    }

    class Balance
    {
        public string? Name { get; set; }
        public double Amount { get; set; }
    }

    internal interface IMarketApi
    {
        public Task<TradingPair[]> GetTradingPairs();
        public Task<Fees[]> GetTradingFees(TradingPair tradingPair);
        public Task<Fees[]> GetWithdrawalFees(TradingPair tradingPair);
        public Task<Balance[]> GetAccountBalance();
    }
}
