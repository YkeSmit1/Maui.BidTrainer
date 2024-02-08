#pragma once

#include <tuple>
#include <string>
#include <vector>
#include <unordered_map>

struct HandCharacteristic;
struct BoardCharacteristic;

class ISqliteWrapper
{
public:
    virtual ~ISqliteWrapper() = default;
    virtual std::tuple<int, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const std::string& previousBidding) = 0;
    virtual std::tuple<int, std::string> GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const std::string& previousBidding) = 0;
    virtual std::string GetRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual std::vector<std::unordered_map<std::string, std::string>> GetInternalRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual std::string GetRelativeRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual std::vector<std::unordered_map<std::string, std::string>> GetInternalRelativeRulesByBid(int bidId, const std::string& previousBidding) = 0;
    virtual void SetModules(int modules) = 0;
};

