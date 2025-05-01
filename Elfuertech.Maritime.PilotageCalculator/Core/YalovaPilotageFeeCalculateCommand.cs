namespace Elfuertech.Maritime.PilotageCalculator.Core;

public class YalovaPilotageFeeCalculateCommand
{
    public string ShippingType { get; set; }
    
    public int GrossTon { get; set; }

    public bool IsVesselCarryingHazardousMaterial { get; set; }

    public bool IsThereMachineFailureAtVessel { get; set; }

    public bool IsPublicHoliday { get; set; }

    public bool IsMaintenance { get; set; }

    public bool IsVesselTurkishFlagged { get; set; }

    public bool OnSiteShifting { get; set; }

    public int NumberOfAdditionalTugsAtTheEntrance { get; set; }

    public int NumberOfAdditionalTugsAtTheExit { get; set; }
}