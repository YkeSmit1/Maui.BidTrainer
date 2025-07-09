namespace Maui.BidTrainer.Services;

public class SettingsService
{
    public event EventHandler SettingsChanged;
    
    public void NotifySettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}