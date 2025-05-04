namespace Elfuertech.PilotageCalculator.Core;

public class PilotageCalculationResponse : BaseResponse
{
    public PilotageCalculationPricing InOutPricing { get; set; }
    public PilotageCalculationPricing PilotagePricing { get; set; }
    public PilotageCalculationPricing MooringPricing { get; set; }
    public PilotageCalculationPricing TugPricing { get; set; }
    public PilotageCalculationPricing ShiftingWithMainEnginePowerBerthToFromDdPricing { get; set; }
    public PilotageCalculationPricing ShiftingWithoutMainEnginePowerBerthToFromDdPricing { get; set; }
}