using Common;
using EngineWrapper;

namespace Maui.BidTrainer.Services;

public class BoardService
{
    public class DisplayAlertEventArgs : EventArgs
    {
        public string Title { get; init; }
        public string Message { get; init; }
    }
    
    public class BoardCompletedEventArgs : EventArgs
    {
        public Result Result { get; init; }
        public Bid Contract { get; init; }
    }

    private readonly BidService bidService;
    
    private Result currentResult;
    private DateTime startTimeBoard;
    public bool IsInHintMode { get; set; }

    private Auction auction;
    private Dictionary<Player, string> deal;
    
    public event EventHandler<DisplayAlertEventArgs> DisplayAlertRequested;
    public event EventHandler<BoardCompletedEventArgs> BoardCompleted;
    public event EventHandler<string> AuctionBidAdded;
    public event EventHandler AuctionCleared;

    public BoardService(BidService bidService)
    {
        this.bidService = bidService;
        bidService.OnDoBid += OnDoBid;
    }

    private async void OnDoBid(object sender, Bid bid)
    {
        await ClickBiddingBoxButton(bid);
    }

    public void StartBoard(BoardDto pbnBoard)
    {
        deal = pbnBoard.Deal;
        auction = new Auction();
        auction.Clear(pbnBoard.Dealer);
        currentResult = new Result();
        startTimeBoard = DateTime.Now;
        IsInHintMode = false;
        AuctionCleared?.Invoke(this, EventArgs.Empty);
        bidService.Auction = auction;
        bidService.AuctionHasChanged();
        BidTillSouth();
    }
    
    private Task ClickBiddingBoxButton(Bid bid)
    {
        try
        {
            try
            {
                if (IsInHintMode)
                {
                    currentResult.UsedHint = true;
                    RaiseBidAlertEvent(BidManager.GetInformation(bid, auction), "Information");
                }
                else
                {
                    var engineBid = BidManager.GetBid(auction, deal[Player.South]);

                    if (bid != engineBid)
                    {
                        var message = $"The correct bid is {engineBid}. Description: {engineBid.description}.";
                        var engineBidInformation = BidManager.GetInformation(engineBid, auction);
                        var bidInformation = BidManager.GetInformation(bid, auction);
                        var s = $"{message}\n\nCorrect bid {engineBid}\n{engineBidInformation}\n\nYour bid {bid}\n{bidInformation}";
                        RaiseBidAlertEvent(s, "Incorrect bid");
                        currentResult.AnsweredCorrectly = false;
                    }
                    UpdateBidControls(engineBid);
                    BidTillSouth();
                }
            }
            catch (Exception exception)
            {
                RaiseBidAlertEvent(exception.Message, "Error");
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private void RaiseBidAlertEvent(string s, string title)
    {
        DisplayAlertRequested?.Invoke(this, new DisplayAlertEventArgs { Title = title, Message = s });
    }

    private void UpdateBidControls(Bid bid)
    {
        auction.AddBid(bid);
        AuctionBidAdded?.Invoke(this, bid.ToString());
        bidService.AuctionHasChanged();
    }

    private void BidTillSouth()
    {
        while (auction.CurrentPlayer != Player.South && !auction.IsEndOfBidding())
        {
            var bid = BidManager.GetBid(auction, deal[auction.CurrentPlayer]);
            UpdateBidControls(bid);
        }

        if (auction.IsEndOfBidding())
        {
            currentResult.TimeElapsed = DateTime.Now - startTimeBoard;
            BoardCompleted?.Invoke(this, new BoardCompletedEventArgs { Result = currentResult, Contract = auction.currentContract});
        }
    }
}    
