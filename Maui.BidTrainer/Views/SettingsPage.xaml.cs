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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ((SettingsViewModel)BindingContext).Save();
        settingsService.NotifySettingsChanged();
    }

    private async void UsernameUnfocused(object sender, FocusEventArgs e)
    {
        try
        {
            var settingsViewModel = (SettingsViewModel)BindingContext;
            if (Preferences.Get("Username", "") == settingsViewModel.Username) 
                return;
            var account = await DependencyService.Get<ICosmosDbHelper>().GetAccount(settingsViewModel.Username);
            if (account is { id: not null })
            {
                await DisplayAlert("Error", "Username already exists", "OK");
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}