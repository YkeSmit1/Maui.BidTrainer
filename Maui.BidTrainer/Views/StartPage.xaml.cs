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
        base.OnAppearing();
        await ((StartViewModel)BindingContext).LoadLessonsAsync();
    }
}