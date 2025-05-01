using Elfuertech.Maritime.PilotageCalculator.Core;

namespace Elfuertech.Maritime.PilotageCalculator;

public partial class ResultPage
{
    private object _command;

    public ResultPage(object command, PilotageCalculationResponse result)
    {
        InitializeComponent();
        _command = command;

        LblSummary.Text = $"Seçilen Parametreler:\n{System.Text.Json.JsonSerializer.Serialize(_command, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";

        if (result.Succeeded)
        {
            LblInOut.Text = $"In/Out: {result.InOutPricing.NetPrice} {result.InOutPricing.CurrencyCode}";
            LblPilotage.Text = $"Pilotaj: {result.PilotagePricing.NetPrice} {result.PilotagePricing.CurrencyCode}";
            LblMooring.Text = $"Yanaşma: {result.MooringPricing.NetPrice} {result.MooringPricing.CurrencyCode}";
            LblTug.Text = $"Römorkör: {result.TugPricing.NetPrice} {result.TugPricing.CurrencyCode}";
            LblShiftMain.Text = $"Ana Makine ile Shift: {result.ShiftingWithMainEnginePowerBerthToFromDdPricing.NetPrice} {result.ShiftingWithMainEnginePowerBerthToFromDdPricing.CurrencyCode}";
            LblShiftNoMain.Text = $"Ana Makinesiz Shift: {result.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.NetPrice} {result.ShiftingWithoutMainEnginePowerBerthToFromDdPricing.CurrencyCode}";
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