using Maui.BidTrainer.Services;
using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class SettingsPage
{
    private readonly SettingsService settingsService;
    public SettingsPage(SettingsService settingsService)
    {
        InitializeComponent();
        this.settingsService = settingsService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((SettingsViewModel)BindingContext).Load();
    }

    protected override async void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();
            var settingsViewModel = (SettingsViewModel)BindingContext;
            if (Preferences.Get("Username", "") != settingsViewModel.Username)
            {
                var account = await DependencyService.Get<ICosmosDbHelper>().GetAccount(settingsViewModel.Username);
                if (account != null)
                {
                    await DisplayAlert("Error", "Username already exists", "OK");
                    return;
                }
            }

            settingsViewModel.Save();
            settingsService.NotifySettingsChanged();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}