// ReSharper disable CppLocalVariableMayBeConst
// ReSharper disable CppCStyleCast
#include "InformationFromAuction.h"
#include <algorithm>
#include "Utils.h"
#include "ISqliteWrapper.h"
#include "nlohmann/json.hpp"

using namespace std::literals::string_literals;

InformationFromAuction::InformationFromAuction(ISqliteWrapper* sqliteWrapper, const std::string& previousBidding)
{
    std::vector minSuitLengths{ std::vector{0, 0, 0, 0}, std::vector{0, 0, 0, 0}, std::vector{0, 0, 0, 0}, std::vector{0, 0, 0, 0} };

    auto bidIds = Utils::SplitAuction(previousBidding);
    auto position = 1;
    std::string currentBidding = "";
    auto partner = ((int)bidIds.size() + 2) % 4;

    for (auto& bidId : bidIds)
    {
        if (bidId != 0)
        {
            auto player = (position - 1) % 4;
            bool isPartner = player == partner;
            if (!isSlamBidding)
            {
                auto rules = sqliteWrapper->GetInternalRulesByBid(bidId, currentBidding);
                if (rules.size() > 0)
                {
                    for (auto i = 0; i < 4; i++)
                        minSuitLengths.at(player).at(i) = std::max(minSuitLengths.at(player).at(i), GetLowestValue(rules, "Min"s + Utils::GetSuit(i) + "s"));
                    if (isPartner)
                        minHcpPartner = std::max(minHcpPartner, GetLowestValue(rules, "MinHcp"));
                }
                else // Check if there is a bid in the slambidding
                {
                    if (ExtraInfoFromRelativeRules(sqliteWrapper, bidId, "", isPartner))
                    {
                        auto trumpSuit = Utils::GetSuitInt(bidIds.at((size_t)(position - 3)));
                        // Slambidding promises fit. So update minSuit
                        if (isPartner)
                            minSuitLengths.at(player).at(trumpSuit) = std::max(minSuitLengths.at(player).at(trumpSuit), 4);
                        isSlamBidding = true;
                        currentBidding = "";
                    }
                }
            }
            else
                ExtraInfoFromRelativeRules(sqliteWrapper, bidId, currentBidding, isPartner);
        }
        currentBidding += Utils::GetBidASCII(bidId);
        position++;
    }

    //TODO Ugly hack
    if (bidIds.size() % 2 != 0)
        isSlamBidding = false;
    if (isSlamBidding)
        previousSlamBidding = currentBidding;

    partnersSuits = minSuitLengths.at(partner);
    openersSuits = minSuitLengths.at(0);
}

bool InformationFromAuction::ExtraInfoFromRelativeRules(ISqliteWrapper* sqliteWrapper, int bidId, const std::string& currentBidding, bool isPartner)
{
    if (auto rules = sqliteWrapper->GetInternalRelativeRulesByBid(bidId, currentBidding); rules.size() > 0)
    {
        if (isPartner)
        {
            for (auto i = 0; i < 4; i++)
                controls.at(i) = controls.at(i) || AllTrue(rules, Utils::GetSuit(i) + "Control"s);
            keyCardsPartner = std::max(keyCardsPartner, GetLowestValue(rules, "KeyCards"));
            trumpQueenPartner = trumpQueenPartner || AllTrue(rules, "TrumpQueen");
        }
        return true;
    }
    return false;
}

int InformationFromAuction::GetLowestValue(const std::vector<std::unordered_map<std::string, std::string>>& rules, const std::string& columnName)
{
    if (rules.size() == 0)
        return 0;
    if (std::all_of(rules.begin(), rules.end(), [&](const auto& a) {return a.at(columnName) == ""; }))
        return 0;

    auto minElement = std::min_element(rules.begin(), rules.end(), [&](const auto& a, const auto& b) {return std::stoi(a.at(columnName)) < std::stoi(b.at(columnName)); });
    auto &value = minElement->at(columnName);
    return value == "" ? 0 : std::stoi(value);
}

bool InformationFromAuction::AllTrue(const std::vector<std::unordered_map<std::string, std::string>>& rules, const std::string& columnName)
{
    if (rules.size() == 0)
        return false;
    auto s = std::all_of(rules.begin(), rules.end(), [&](const auto& a) {return a.at(columnName) != "" && std::stoi(a.at(columnName)) == 1; });
    return s;
}

std::string InformationFromAuction::AsJson()
{
    nlohmann::json j
    {
        {"minSuitLengthsPartner", partnersSuits},
        {"minHcpPartner", minHcpPartner},
        {"controls", controls},
        {"keyCardsPartner", keyCardsPartner},
        {"trumpQueenPartner", trumpQueenPartner},
    };

    std::stringstream ss;
    ss << j;
    return ss.str();
}
