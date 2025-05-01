using Elfuertech.Maritime.PilotageCalculator.Core;

namespace Elfuertech.Maritime.PilotageCalculator;

public partial class ResultPage
{
    public ResultPage(object command, PilotageCalculationResponse result)
    {
        InitializeComponent();

        if (result.Succeeded)
        {
            LblInOut.Text = $"{result.InOutPricing.NetPrice} {result.InOutPricing.CurrencyCode}";
            LblPilotage.Text = $"{result.PilotagePricing.NetPrice} {result.PilotagePricing.CurrencyCode}";
            LblMooring.Text = $"{result.MooringPricing.NetPrice} {result.MooringPricing.CurrencyCode}";
            LblTug.Text = $"{result.TugPricing.NetPrice} {result.TugPricing.CurrencyCode}";
            LblShiftingWithEngine.Text = $"{result.ShiftingWithMainEnginePowerBerthToFromDdPricing.NetPrice} {result.ShiftingWithMainEnginePowerBerthToFromDdPricing.CurrencyCode}";
            LblShiftingWithoutEngine.Text = $"{result.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.NetPrice} {result.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.CurrencyCode}";
            return;
        }

        LblError.IsVisible = true;
        LblError.Text = result.ErrorDetail;
    }

    private async void OnRecalculateClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}