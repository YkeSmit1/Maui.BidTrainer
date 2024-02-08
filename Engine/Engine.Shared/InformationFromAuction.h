#pragma once

#include <vector>
#include <string>
#include <unordered_map>

class ISqliteWrapper;

struct InformationFromAuction
{
    InformationFromAuction(ISqliteWrapper* sqliteWrapper, const std::string& previousBidding);
    InformationFromAuction() {};

    std::vector<int> partnersSuits{0,0,0,0};
    std::vector<int> openersSuits{0,0,0,0};
    int minHcpPartner = 0;

    std::vector<bool> controls{false, false, false, false};
    std::string previousSlamBidding{};
    bool isSlamBidding = false;
    int keyCardsPartner = 0;
    bool trumpQueenPartner = false;
    std::string AsJson();
private:
    int GetLowestValue(const std::vector<std::unordered_map<std::string, std::string>>& rules, const std::string& columnName);
    bool AllTrue(const std::vector<std::unordered_map<std::string, std::string>>& rules, const std::string& columnName);
    bool ExtraInfoFromRelativeRules(ISqliteWrapper* sqliteWrapper, int bidId, const std::string& currentBidding, bool isPartner);
};

