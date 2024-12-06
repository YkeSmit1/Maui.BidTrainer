namespace Maui.BidTrainer;

public partial class App
{
    public App()
    {
            InitializeComponent();

            MainPage = new AppShell();
        }
        
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

#if WINDOWS
        window.Width = 600;
        window.Height = 1000;
        
        var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
        window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
        window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
#endif
        return window;
    }    
        
}