namespace Elfuertech.Maritime.PilotageCalculator.Core;

public class TuzlaGisasPricePackage(int grtIntervalBegin, int grtIntervalEnd)
{
    public int GrtIntervalBegin { get;  } = grtIntervalBegin;

    public int GrtIntervalEnd { get; } = grtIntervalEnd;

    public Dictionary<string, TuzlaGisasPrice> PriceDictionary { get; set; } = new();

    public TuzlaGisasConstant Constants { get; set; }
}
