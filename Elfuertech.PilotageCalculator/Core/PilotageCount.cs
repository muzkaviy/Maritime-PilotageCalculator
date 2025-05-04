namespace Elfuertech.PilotageCalculator.Core;

public class PilotageCount(int onTheEntrance, int onTheExit)
{
    public int TotalCount => onTheEntrance + onTheExit;
}