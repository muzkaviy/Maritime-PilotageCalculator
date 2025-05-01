namespace Elfuertech.Maritime.PilotageCalculator.Core;

public class PilotageCalculationDto
{
    public PilotageCount InOutCount { get; set; }
    public PilotagePricing InOutPricing { get; set; }
    public PilotageCount PilotageCount { get; set; }
    public PilotagePricing PilotagePricing { get; set; }
    public PilotageCount MooringCount { get; set; }
    public PilotagePricing MooringPricing { get; set; }
    public PilotageCount TugCount { get; set; }
    public PilotagePricing TugPricing { get; set; }
    public PilotagePricing AdditionalTugPricing { get; set; }
    public PilotagePricing ShiftingWithMainEnginePowerBerthToFromDdPricing { get; set; }
    public PilotagePricing ShiftingWithoutMainEnginePowerBerthToFromDdPricing { get; set; }
}