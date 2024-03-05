using System.Collections.ObjectModel;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maui.BidTrainer.ViewModels
{
    public class BiddingBoxViewModel : ObservableObject
    {
        public ObservableCollection<Bid> SuitBids { get; set; }
        public ObservableCollection<Bid> NonSuitBids { get; set; }
        public AsyncRelayCommand<object> DoBid { get; set; }

        public BiddingBoxViewModel()
        {
            SuitBids = new ObservableCollection<Bid>(Enum.GetValues(typeof(Suit)).Cast<Suit>()
                            .SelectMany(_ => Enumerable.Range(1, 7), (suit, level) => new { suit, level })
                            .Select(x => new Bid(x.level, x.suit)));
            NonSuitBids = [Bid.PassBid, Bid.Dbl, Bid.Rdbl];
        }
    }
}
