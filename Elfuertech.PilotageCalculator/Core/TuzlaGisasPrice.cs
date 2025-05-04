namespace Elfuertech.PilotageCalculator.Core;

public class TuzlaGisasPrice(decimal pilotage, decimal tug, decimal mooring)
{
    public decimal Pilotage { get; set; } = pilotage;
    public decimal Tug { get; set; } = tug;
    public decimal Mooring { get; set; } = mooring;
}