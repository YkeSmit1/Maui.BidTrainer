using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private Results results = new();
}