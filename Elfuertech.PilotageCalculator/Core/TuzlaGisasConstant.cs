namespace Elfuertech.PilotageCalculator.Core;

public class TuzlaGisasConstant(decimal skil, decimal demir, decimal rs, decimal cer, decimal @ref, decimal pilot, decimal rombek)
{
    public decimal Skil { get; set; } = skil;
    public decimal Demir { get; set; } = demir;
    public decimal Rs { get; set; } = rs;
    public decimal Cer { get; set; } = cer;
    public decimal Ref { get; set; } = @ref;
    public decimal Pilot { get; set; } = pilot;
    public decimal Rombek { get; set; } = rombek;
}