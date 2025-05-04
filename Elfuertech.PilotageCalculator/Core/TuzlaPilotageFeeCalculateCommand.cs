namespace Elfuertech.PilotageCalculator.Core;

public class TuzlaPilotageFeeCalculateCommand
{
    public string ShippingType { get; set; }
    
    public int GrossTon { get; set; }

    public bool IsVesselCarryingHazardousMaterial { get; set; }

    public bool IsThereMachineFailureAtVessel { get; set; }

    public bool IsPublicHoliday { get; set; }

    public bool IsMaintenance { get; set; }

    public bool IsVesselTurkishFlagged { get; set; }

    public bool OnSiteShifting { get; set; }
    
    public bool IsGasFree { get; set; }

    public bool ManeuverCancellation { get; set; }
}