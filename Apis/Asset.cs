using System;
using CryptoTA.Database.Models;

namespace CryptoTA.Apis
{
    public class Asset
    {
        public string Name { get; set; }
        public string Altname { get; set; }
        public int Decimals { get; set; }
    }
}
