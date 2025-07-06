using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class AuctionViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<string> Bids {get; set;} = [];
}