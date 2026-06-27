using Common;

namespace Maui.BidTrainer.Services;

public class BidService
{
    public event EventHandler<Bid> OnDoBid;
    public event EventHandler OnAuctionHasChanged;
    public Auction Auction { get; set; }
    
    public void DoBid(Bid bid)
    {
        OnDoBid?.Invoke(this, bid);
    }
    
    public void AuctionHasChanged()
    {
        OnAuctionHasChanged?.Invoke(this, EventArgs.Empty);
    }
}