using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class AuctionViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<string> Bids {get; set;} = [];

    public void AddBid(string bid)
    {
        if (Bids.Any() && Bids.Last() == "?")
            Bids.RemoveAt(Bids.Count - 1);
        Bids.Add(bid);
        Bids.Add("?");
    }
    
    public void Clear()
    {
        Bids.Clear();
    }    
}