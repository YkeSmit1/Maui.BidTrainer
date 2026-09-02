using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class TheoryViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] public partial int Lesson {get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Lesson = int.Parse((string)query["Lesson"]);
    }
    
    [RelayCommand]
    private async Task Close()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(BidTrainerPage)}?PracticeMode={false}");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Error", exception.ToString(), "OK");
        }
    }
    
}