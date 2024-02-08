#pragma once
#include <vector>
#include <string>
#include <sstream>

class Utils
{
public:
    template<typename CharType>
    static std::vector<std::basic_string<CharType>> Split(const std::basic_string<CharType>& str, const CharType& delimiter);
    static std::string GetSuitFromBidId(int bidId);
    static std::string GetSuit(int suit);
    static int GetSuitInt(int bidId);
    static int GetSuit(const std::string& suit);
    static std::string GetSuitASCII(int bidId);
    static std::vector<int> SplitAuction(const std::string& auction);
    static std::vector<std::string> SplitAuctionAsString(const std::string& auction);
    static int GetBidId(const std::string& bid);
    static int GetRank(int bidId);
    static int NumberOfCards(const std::string& hand, char card);
    static int CalculateHcp(const std::string& hand);
    static bool GetIsCompetitive(const std::string& bidding);
    static std::string GetBidASCII(int bidId);
    static int GetLastBidIdFromAuction(const std::string& bidding);
    static std::string GetLastBidFromAuction(const std::string& bidding);
};

template<typename CharType>
std::vector<std::basic_string<CharType>> Utils::Split(const std::basic_string<CharType>& str, const CharType& delimiter)
{
    std::vector<std::basic_string<CharType>> subStrings;
    std::basic_stringstream<CharType> stringStream(str);
    std::basic_string<CharType> item;
    while (getline(stringStream, item, delimiter))
    {
        subStrings.push_back(move(item));
    }

    while (subStrings.size() < 4)
        subStrings.emplace_back("");

    return subStrings;
}
