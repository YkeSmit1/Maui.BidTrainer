using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class ResultsViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    public partial Results Results {get; set; } = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Results = (Results)query["Results"];
    }
}