using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Services;

namespace Maui.BidTrainer.ViewModels;

public partial class BidViewModel : ObservableObject
{
    private readonly BidService bidService;

    public Bid Bid { get; }
    public string BidString => Bid.ToString();
    public Color Color =>
        !CanDoBid() ? Colors.Gray :
        Bid.Suit is Suit.Diamonds or Suit.Hearts ? Colors.Red :
        Application.Current is { RequestedTheme: AppTheme.Light } ? Colors.Black : Colors.White;

    public BidViewModel(Bid bid, BidService bidService)
    {
        Bid = bid;
        this.bidService = bidService;
        bidService.OnAuctionHasChanged += (_, _) => AuctionChanged();
    }
    
    private void AuctionChanged()
    {
        DoBidCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(Color));
    }

    private bool CanDoBid()
    {
        return bidService?.Auction?.BidIsPossible(Bid) ?? false;
    }

    [RelayCommand(CanExecute = nameof(CanDoBid))]
    private void DoBid()
    {
        bidService.DoBid(Bid);
    }
}