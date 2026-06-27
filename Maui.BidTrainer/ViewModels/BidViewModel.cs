using Common;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Services;

namespace Maui.BidTrainer.ViewModels;

public partial class BidViewModel
{
    private readonly BidService bidService =
        Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<BidService>()
        ?? throw new InvalidOperationException("BidService is not registered.");

    public Bid Bid { get; }
    public string BidString => Bid.ToString();

    public BidViewModel(Bid bid)
    {
        Bid = bid;
        bidService.OnAuctionHasChanged += (_, _) => AuctionChanged();
    }
    
    private void AuctionChanged()
    {
        DoBidCommand.NotifyCanExecuteChanged();
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