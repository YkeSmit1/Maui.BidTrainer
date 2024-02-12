#pragma once

#ifdef _WIN32
#  define DLLENGINE_API __declspec( dllexport )
#else
#  define DLLENGINE_API
#endif


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
    DLLENGINE_API int GetBidFromRule(const char* hand, const char* previousBidding, char* description);
    DLLENGINE_API void GetRulesByBid(int bidId, const char* previousBidding, char* information);
    DLLENGINE_API int Setup(const char* database);
    DLLENGINE_API void SetModules(int modules);
    DLLENGINE_API void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionJson);
}