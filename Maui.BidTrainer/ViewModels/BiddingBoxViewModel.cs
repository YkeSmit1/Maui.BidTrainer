using System.Collections.ObjectModel;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class BiddingBoxViewModel : ObservableObject
{
    public ObservableCollection<BidViewModel> SuitBids { get; set; }
    public ObservableCollection<BidViewModel> NonSuitBids { get; set; }

    public BiddingBoxViewModel()
    {
        var suitBids = Enum.GetValues<Suit>()
            .SelectMany(_ => Enumerable.Range(1, 7), (suit, level) => new { suit, level })
            .Select(x => new Bid(x.level, x.suit)).OrderBy(x => x.Suit).ThenBy(x => x.Rank);
        SuitBids = new ObservableCollection<BidViewModel>(suitBids.Select(x => new BidViewModel(x)));
        List<Bid> bids = [Bid.PassBid, Bid.Dbl, Bid.Rdbl];
        NonSuitBids = new ObservableCollection<BidViewModel>(bids.Select(x => new BidViewModel(x)));
    }
}