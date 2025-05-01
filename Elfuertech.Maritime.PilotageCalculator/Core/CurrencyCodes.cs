

// ReSharper disable InconsistentNaming

namespace Elfuertech.Maritime.PilotageCalculator.Core;

public static class CurrencyCodes
{
    public const string TRY = nameof(TRY);
    public const string USD = nameof(USD);
    public const string EUR = nameof(EUR);
    public const string AED = nameof(AED);

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        TRY,
        USD,
        EUR,
        AED
    };
}