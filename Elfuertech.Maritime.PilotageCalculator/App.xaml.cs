namespace Elfuertech.Maritime.PilotageCalculator;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navigationPage = new NavigationPage(new MainPage());
        return new Window(navigationPage);
    }
}