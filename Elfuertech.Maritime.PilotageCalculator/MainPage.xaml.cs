namespace Elfuertech.Maritime.PilotageCalculator;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    
    private async void OnNavigateToCalculationClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalculationPage(""));
    }
}