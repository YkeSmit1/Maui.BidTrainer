using System.Reflection;
using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            ImageDefault.Source = ImageSource.FromResource("Maui.BidTrainer.Resources.cardfaces.png", typeof(SettingsPage).GetTypeInfo().Assembly);
            ImageBbo.Source = ImageSource.FromResource("Maui.BidTrainer.Resources.cardfaces2.jpg", typeof(SettingsPage).GetTypeInfo().Assembly);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((SettingsViewModel)BindingContext).Load();
        }

        protected override async void OnDisappearing()
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
    }
}