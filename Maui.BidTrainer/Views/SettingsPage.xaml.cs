using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class SettingsPage
{
    public SettingsPage()
    {
        InitializeComponent();
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
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void ButtonSave_OnClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PopModalAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}