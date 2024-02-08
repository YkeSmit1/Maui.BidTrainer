// ReSharper disable CppParameterMayBeConst
// ReSharper disable CppLocalVariableMayBeConst
// ReSharper disable CppCStyleCast
#include "Utils.h"
#include <algorithm>

std::string Utils::GetSuitFromBidId(int bidId)
{
    return GetSuit(GetSuitInt(bidId));
}

std::string Utils::GetSuit(int suit)
{
    switch (suit)
    {
    case 0: return "Spade";
    case 1: return "Heart";
    case 2: return "Diamond";
    case 3: return "Club";
    default:
        throw std::invalid_argument("Unknown suit");
    }
}


int Utils::GetSuitInt(int bidId)
{
    return 4 - (bidId % 5);
}

int Utils::GetSuit(const std::string& suit)
{
    if (suit == "NT")
        return -1;
    if (suit == "S")
        return 0;
    if (suit == "H")
        return 1;
    if (suit == "D")
        return 2;
    if (suit == "C")
        return 3;
    throw std::invalid_argument("Unknown suit");
}

std::string Utils::GetSuitASCII(int bidId)
{
    switch (GetSuitInt(bidId))
    {
    case 0: return "S";
    case 1: return "H";
    case 2: return "D";
    case 3: return "C";
    case 4: return "NT";
    default:
        throw std::invalid_argument("Unknown suit");
    }
}

std::vector<int> Utils::SplitAuction(const std::string& auction)
{
    std::vector<int> ret{};
    std::string currentBid = "";
    for (auto& c : auction)
    {
        if (currentBid.length() > 0 && (isdigit(c) || c == 'P' || c == 'X'))
        {
            if (currentBid != "")
                ret.push_back(GetBidId(currentBid));
            currentBid = "";
        }
        currentBid += c;
    }
    if (currentBid != "")
        ret.push_back(GetBidId(currentBid));

    return ret;
}

std::vector<std::string> Utils::SplitAuctionAsString(const std::string& auction)
{
    std::vector<std::string> ret{};
    std::string currentBid = "";
    for (auto& c : auction)
    {
        if (currentBid.length() > 0 && (isdigit(c) || c == 'P' || c == 'X'))
        {
            if (currentBid != "")
                ret.push_back(currentBid);
            currentBid = "";
        }
        currentBid += c;
    }
    if (currentBid != "")
        ret.push_back(currentBid);

    return ret;
}

int Utils::GetBidId(const std::string& bid)
{
    if (bid == "Pass")
        return 0;
    if (bid == "X")
        return -1;
    if (bid == "XX")
        return -2;
    auto rank = std::stoi(bid.substr(0, 1));
    auto suit = GetSuit(bid.substr(1, bid.length() - 1));
    return (rank - 1) * 5 + 4 - suit;
}


int Utils::GetRank(int bidId)
{
    return (int)((bidId - 1) / 5) + 1;
}

int Utils::NumberOfCards(const std::string& hand, char card)
{
    return (int)std::count_if(hand.begin(), hand.end(), [card](auto c) {return c == card; });
}

int Utils::CalculateHcp(const std::string& hand)
{
    const auto aces = Utils::NumberOfCards(hand, 'A');
    const auto kings = Utils::NumberOfCards(hand, 'K');
    const auto queens = Utils::NumberOfCards(hand, 'Q');
    const auto jacks = Utils::NumberOfCards(hand, 'J');
    return aces * 4 + kings * 3 + queens * 2 + jacks;
}

bool Utils::GetIsCompetitive(const std::string& bidding)
{
    auto bidIds = Utils::SplitAuction(bidding);

    std::vector<int> bidsOpenerTeam{ };
    std::vector<int> bidsOtherTeam{ };

    auto isOpenerTeam = true;
    for (auto& bidId : bidIds)
    {
        if (isOpenerTeam)
            bidsOpenerTeam.push_back(bidId);
        else
            bidsOtherTeam.push_back(bidId);
        isOpenerTeam = !isOpenerTeam;
    }
    return std::any_of(bidsOpenerTeam.begin(), bidsOpenerTeam.end(), [](auto bidId) {return bidId > 0; }) &&
        std::any_of(bidsOtherTeam.begin(), bidsOtherTeam.end(), [](auto bidId) {return bidId > 0; });
}

std::string Utils::GetBidASCII(int bidId)
{
    if (bidId == 0)
        return "Pass";
    if (bidId == -1)
        return "X";
    return std::to_string(GetRank(bidId)) + GetSuitASCII(bidId);
}

int Utils::GetLastBidIdFromAuction(const std::string& bidding)
{
    auto bidIds = Utils::SplitAuction(bidding);
    auto lastBidding = std::find_if(bidIds.rbegin(), bidIds.rend(), [](auto bidId) {return bidId > 0; });
    auto lastBidId = lastBidding == bidIds.rend() ? 0 : *lastBidding;
    return lastBidId;
}

std::string Utils::GetLastBidFromAuction(const std::string& bidding)
{
    return GetBidASCII(GetLastBidIdFromAuction(bidding));
}