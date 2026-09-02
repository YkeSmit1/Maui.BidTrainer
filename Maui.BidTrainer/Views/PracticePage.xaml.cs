using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class PracticePage
{
    public PracticePage(PracticeViewModel practiceViewModel)
    {
        InitializeComponent();
        BindingContext = practiceViewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await ((PracticeViewModel)BindingContext).LoadLessonsAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Error", exception.Message, "OK");
        }
    }
}
