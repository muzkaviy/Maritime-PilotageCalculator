namespace Elfuertech.PilotageCalculator.Core;

public static class Ports
{
    public const string Tuzla = nameof(Tuzla);
    public const string Yalova = nameof(Yalova);
    
    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Tuzla,
        Yalova
    };
}