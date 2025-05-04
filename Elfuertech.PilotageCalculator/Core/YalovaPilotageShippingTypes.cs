namespace Elfuertech.PilotageCalculator.Core;

public static class YalovaPilotageShippingTypes
{
    public const string RoRo = nameof(RoRo);
    public const string CabotageLine = nameof(CabotageLine);
    public const string Container = nameof(Container);
    public const string Other = nameof(Other);

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        RoRo,
        CabotageLine,
        Container,
        Other
    };
}