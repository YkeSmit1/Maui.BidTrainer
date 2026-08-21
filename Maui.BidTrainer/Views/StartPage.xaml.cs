using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class StartPage
{
    public StartPage(StartViewModel startViewModel)
    {
        InitializeComponent();
        BindingContext = startViewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await ((StartViewModel)BindingContext).LoadLessonsAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Error", exception.Message, "OK");
        }
    }
}