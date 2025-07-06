using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Username {get; set; }
    [ObservableProperty]
    public partial bool AlternateSuits {get; set; }
    [ObservableProperty]
    public partial string CardImage {get; set; }

    public SettingsViewModel()
    {
        Load();
    }

    public void Load()
    {
        Username = Preferences.Get("Username", "");
        AlternateSuits = Preferences.Get("AlternateSuits", true);
        CardImage = Preferences.Get("CardImageSettings", "default");
    }

    public void Save()
    {
        Preferences.Set("Username", Username);
        Preferences.Set("AlternateSuits", AlternateSuits);
        Preferences.Set("CardImageSettings", CardImage);
    }
}