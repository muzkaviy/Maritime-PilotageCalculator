using Elfuertech.Maritime.PilotageCalculator.Business;
using Elfuertech.Maritime.PilotageCalculator.Core;

namespace Elfuertech.Maritime.PilotageCalculator;

public partial class CalculationPage
{
    private string _port;

    public CalculationPage(string port)
    {
        _port = port;
        InitializeComponent();
    }

    private void OnPortSelectionChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value)
        {
            return;
        }

        var radio = (RadioButton)sender;

        _port = radio.Value.ToString() ?? string.Empty;

        var isTuzla = _port == Ports.Tuzla;
        var isYalova = _port == Ports.Yalova;

        LayoutGasFree.IsVisible = isTuzla;
        LayoutCancel.IsVisible = isTuzla;
        LayoutTugIn.IsVisible = isYalova;
        LayoutTugOut.IsVisible = isYalova;
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        var selectedRadio = GridShippingType.Children
            .OfType<RadioButton>()
            .FirstOrDefault(rb => rb.IsChecked);

        var selectedShippingType = selectedRadio?.Value.ToString() ?? string.Empty;

        _ = int.TryParse(EntryGrossTon.Text, out var grossTon);

        if (_port == Ports.Tuzla)
        {
            var cmd = new TuzlaPilotageFeeCalculateCommand
            {
                ShippingType = selectedShippingType,
                GrossTon = grossTon,
                IsVesselCarryingHazardousMaterial = ChkHazardous.IsChecked,
                IsThereMachineFailureAtVessel = ChkFailure.IsChecked,
                IsPublicHoliday = ChkHoliday.IsChecked,
                IsMaintenance = ChkMaintenance.IsChecked,
                IsVesselTurkishFlagged = ChkTurkishFlag.IsChecked,
                OnSiteShifting = ChkShifting.IsChecked,
                IsGasFree = ChkGasFree.IsChecked,
                ManeuverCancellation = ChkCancel.IsChecked
            };

            var tuzlaPilotageFeeService = new TuzlaPilotageFeeService();
            var response = tuzlaPilotageFeeService.Calculate(cmd);
            
            await Navigation.PushAsync(new ResultPage(cmd, response));
        }
        else if (_port == Ports.Yalova)
        {
            var cmd = new YalovaPilotageFeeCalculateCommand
            {
                ShippingType = selectedShippingType,
                GrossTon = grossTon,
                IsVesselCarryingHazardousMaterial = ChkHazardous.IsChecked,
                IsThereMachineFailureAtVessel = ChkFailure.IsChecked,
                IsPublicHoliday = ChkHoliday.IsChecked,
                IsMaintenance = ChkMaintenance.IsChecked,
                IsVesselTurkishFlagged = ChkTurkishFlag.IsChecked,
                OnSiteShifting = ChkShifting.IsChecked,
                NumberOfAdditionalTugsAtTheEntrance = int.TryParse(EntryTugIn.Text, out var tin) ? tin : 0,
                NumberOfAdditionalTugsAtTheExit = int.TryParse(EntryTugOut.Text, out var tout) ? tout : 0
            };

            var yalovaPilotageFeeService = new YalovaPilotageFeeService();
            var response = yalovaPilotageFeeService.Calculate(cmd);

            await Navigation.PushAsync(new ResultPage(cmd, response));
        }
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        foreach (var rb in PortRadioGroup.Children.OfType<RadioButton>())
        {
            rb.IsChecked = false;
        }

        foreach (var rb in GridShippingType.Children.OfType<RadioButton>())
        {
            rb.IsChecked = false;
        }

        EntryGrossTon.Text = string.Empty;
        EntryTugIn.Text = string.Empty;
        EntryTugOut.Text = string.Empty;

        ChkHazardous.IsChecked = false;
        ChkFailure.IsChecked = false;
        ChkHoliday.IsChecked = false;
        ChkMaintenance.IsChecked = false;
        ChkTurkishFlag.IsChecked = false;
        ChkShifting.IsChecked = false;
        ChkGasFree.IsChecked = false;
        ChkCancel.IsChecked = false;
        
        LayoutGasFree.IsVisible = false;
        LayoutCancel.IsVisible = false;
        LayoutTugIn.IsVisible = false;
        LayoutTugOut.IsVisible = false;
    }
    
    private void OnScrollViewTapped(object sender, TappedEventArgs e)
    {
        EntryGrossTon.Unfocus();
    }
}