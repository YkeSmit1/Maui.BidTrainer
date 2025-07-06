using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial Results Results {get; set; } = new();
}