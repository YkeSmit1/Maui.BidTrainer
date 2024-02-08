#pragma once

/// <summary>
/// Types of bids deduced from the hand
/// </summary>
enum class BidKind
{
    UnknownSuit,
    FirstSuit,
    SecondSuit,
    LowestSuit,
    HighestSuit,
    PartnersSuit,
};

/// <summary>
/// Type of bids deduced from the auction
/// </summary>
enum class BidKindAuction
{
    UnknownSuit,
    NonReverse,
    Reverse,
    OwnSuit,
    PartnersSuit,
};

enum class Modules
{
    FourCardMajors = 1,
    FiveCardMajors = 2,
    TwoBidsAndHigher = 4,
    NegativeDbl = 8,
    Reverse = 16,
    ControlBids = 32,
    RKC = 64
};

extern "C" {
    int GetBidFromRule(const char* hand, const char* previousBidding, char* description);
    void GetRulesByBid(int bidId, const char* previousBidding, char* information);
    int Setup(const char* database);
    void SetModules(int modules);
    void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionJson);
}