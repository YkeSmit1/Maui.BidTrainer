﻿using System.Collections.ObjectModel;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maui.BidTrainer.ViewModels;

public class BiddingBoxViewModel : ObservableObject
{
    public ObservableCollection<Bid> SuitBids { get; set; }
    public ObservableCollection<Bid> NonSuitBids { get; set; }
    public AsyncRelayCommand<Bid> DoBidCommand { get; set; }

    public BiddingBoxViewModel()
    {
        SuitBids = new ObservableCollection<Bid>(Enum.GetValues<Suit>()
            .SelectMany(_ => Enumerable.Range(1, 7), (suit, level) => new { suit, level })
            .Select(x => new Bid(x.level, x.suit)).OrderBy(x => x.Suit).ThenBy(x => x.Rank));
        NonSuitBids = [Bid.PassBid, Bid.Dbl, Bid.Rdbl];
    }
}