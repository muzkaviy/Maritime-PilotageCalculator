namespace Elfuertech.PilotageCalculator.Core;

public class TugCount(int grtIntervalBegin, int grtIntervalEnd, int count)
{
    public int GrtIntervalBegin { get; set; } = grtIntervalBegin;
    public int GrtIntervalEnd { get; set; } = grtIntervalEnd;
    public int Count { get; set; } = count;
}