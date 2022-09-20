using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CryptoTA.Utils;

public static class CurrencyCodeMapper
{
    private static readonly Dictionary<string, string> SymbolsByCode;

    public static string GetSymbol(string code) 
    {
        if (!SymbolsByCode.ContainsKey(code))
        {
            return code;
        }

        return SymbolsByCode[code];
    }

    public static string AttachSymbol(string code, string value)
    {
        if (!SymbolsByCode.ContainsKey(code))
        {
            return value;
        }

        return $"{SymbolsByCode[code]} {value}";
    }

    static CurrencyCodeMapper()
    {
        SymbolsByCode = new Dictionary<string, string>();

        var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                      .Select(x => new RegionInfo(x.LCID));

        foreach (var region in regions)
        {
            if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
            {
                SymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
            }
        }
    }
}
