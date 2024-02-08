using Common;
using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class AuctionViewModel : BaseViewModel
    {
        private Auction auction = new Auction();
        public Auction Auction { get => auction; set => SetProperty(ref auction, value); }
        public AuctionViewModel()
        {
            Auction.AddBid(Bid.PassBid);
            Auction.AddBid(new Bid(1, Suit.Diamonds));
            Auction.AddBid(new Bid(BidType.dbl));
            Auction.AddBid(new Bid(1, Suit.Spades));
            Auction.AddBid(new Bid(BidType.pass));
            Auction.AddBid(new Bid(2, Suit.Clubs));
            Auction.AddBid(new Bid(BidType.pass));
        }
    }
}
