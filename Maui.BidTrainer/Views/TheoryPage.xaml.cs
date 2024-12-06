namespace Maui.BidTrainer.Views;

public partial class TheoryPage
{
    public TheoryPage()
    {
        InitializeComponent();
    }

    private async void ButtonCloseClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(BidTrainerPage));
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.ToString(), "OK");
        }
    }
}