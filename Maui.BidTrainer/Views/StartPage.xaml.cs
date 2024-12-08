using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class StartPage
{
    public StartPage()
    {
        InitializeComponent();
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
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}