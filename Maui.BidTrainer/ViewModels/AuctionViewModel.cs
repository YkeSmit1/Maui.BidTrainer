using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class AuctionViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> bids = [];
}