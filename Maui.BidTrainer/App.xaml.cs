namespace Maui.BidTrainer;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }
        
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = new Window(new AppShell());

#if WINDOWS
        window.Width = 600;
        window.Height = 900;
        
        var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

        window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
        window.Y = ((displayInfo.Height / displayInfo.Density - window.Height) / 2) - 20;
#endif
        return window;
    }    
        
}