// ReSharper disable CppLocalVariableMayBeConst
// ReSharper disable CppCStyleCast
// ReSharper disable CppParameterMayBeConst
#include <iostream>
#include <vector>
#include <unordered_map>

#include "SqliteCppWrapper.h"
#include "nlohmann/json.hpp"
#include "sqlite3.h"

#include "Rule.h"
#include "Utils.h"
#include "Api.h"
#include "BoardCharacteristic.h"
#include <regex>

static void regex_match(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 2)
    {
        auto* regexp = (const char*)sqlite3_value_text(argv[0]);
        auto* text = (const char*)sqlite3_value_text(argv[1]);
        if (text && text[0] && regexp && regexp[0])
        {
            std::regex regex(regexp);
            auto match = std::regex_search(text, regex);
            sqlite3_result_int(context, match);
            return;
        }
    }
    sqlite3_result_null(context);
}

static void getBidKindFromAuction(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 2)
    {
        auto* previousBidding = (const char*)sqlite3_value_text(argv[0]);
        auto bidId = sqlite3_value_int(argv[1]);
        if (previousBidding && previousBidding[0])
        {
            auto match = (int)SqliteCppWrapper::GetBidKindFromAuction(previousBidding, bidId);
            sqlite3_result_int(context, match);
            return;
        }
    }
    sqlite3_result_null(context);
}

SqliteCppWrapper::SqliteCppWrapper(const std::string& database)
{
    try
    {
        db.release();
        db = std::make_unique<SQLite::Database>(database.c_str());
        db->createFunction("regex_match", 2, true, nullptr, &regex_match, nullptr, nullptr, nullptr);
        db->createFunction("getBidKindFromAuction", 2, true, nullptr, &getBidKindFromAuction, nullptr, nullptr, nullptr);

        queryShape = std::make_unique<SQLite::Statement>(*db, shapeSql.data());
        queryRules = std::make_unique<SQLite::Statement>(*db, rulesSql.data());
        queryShapeRelative = std::make_unique<SQLite::Statement>(*db, relativeShapeSql.data());
        queryRelativeRules= std::make_unique<SQLite::Statement>(*db, relativeRulesSql.data());
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

std::tuple<int, std::string> SqliteCppWrapper::GetRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const std::string& previousBidding)
{
    try
    {
        // Bind parameters
        queryShape->reset();

        queryShape->bind(":firstSuit", hand.firstSuit);
        queryShape->bind(":secondSuit", hand.secondSuit);
        queryShape->bind(":lowestSuit", hand.lowestSuit);
        queryShape->bind(":highestSuit", hand.highestSuit);
        queryShape->bind(":fitWithPartnerSuit", board.fitWithPartnerSuit);

        queryShape->bind(":lastBidId", board.lastBidId);
        queryShape->bind(":minSpades", hand.suitLengths[0]);
        queryShape->bind(":minHearts", hand.suitLengths[1]);
        queryShape->bind(":minDiamonds", hand.suitLengths[2]);
        queryShape->bind(":minClubs", hand.suitLengths[3]);

        queryShape->bind(":minHcp", hand.hcp);
        queryShape->bind(":isBalanced", hand.isBalanced);
        queryShape->bind(":opponentsSuit", (int)board.opponentsSuit);
        queryShape->bind(":stopInOpponentsSuit", board.stopInOpponentsSuit);

        queryShape->bind(":lengthFirstSuit", hand.lengthFirstSuit);
        queryShape->bind(":lengthSecondSuit", hand.lengthSecondSuit);
        queryShape->bind(":hasFit", board.hasFit);
        queryShape->bind(":fitIsMajor", board.fitIsMajor);
        queryShape->bind(":modules", modules);
        queryShape->bind(":position", board.position);
        queryShape->bind(":isCompetitive", board.isCompetitive);
        queryShape->bind(":isReverse", hand.isReverse);
        queryShape->bind(":isSemiBalanced", hand.isSemiBalanced);
        queryShape->bind(":previousBidding", previousBidding);

        while (queryShape->executeStep())
        {
            auto bidId = queryShape->getColumn(0).getInt();
            auto str = queryShape->getColumn(1).getString();

            return std::make_tuple(bidId, str);
        }
        return std::make_tuple(0, "");
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

BidKindAuction SqliteCppWrapper::GetBidKindFromAuction(const std::string& previousBidding, int bidId)
{
    auto bids = Utils::SplitAuction(previousBidding);
    auto lengthAuction = bids.size();

    auto suit = Utils::GetSuitInt(bidId);
    if (HasFitWithPartner(bids, lengthAuction, suit))
        return BidKindAuction::PartnersSuit;

    if (IsRebidOwnSuit(bids, lengthAuction, suit))
        return BidKindAuction::OwnSuit;

    if (lengthAuction >= 4 && suit >= 0 && suit <= 3)
    {
        auto rank = Utils::GetRank(bidId);
        auto previousSuit = Utils::GetSuitInt(bids.at(lengthAuction - 4));
        auto previousRank = Utils::GetRank(bids.at(lengthAuction - 4));

        if (IsNonReverse(suit, rank, previousSuit, previousRank))
            return BidKindAuction::NonReverse;

        if (IsReverse(suit, rank, previousSuit, previousRank))
            return BidKindAuction::Reverse;
    }

    return BidKindAuction::UnknownSuit;
}

bool SqliteCppWrapper::HasFitWithPartner(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return HasFitWithPartnerPrevious(bids, lengthAuction, suit) || HasFitWithPartnerFirst(bids, lengthAuction, suit);
}
bool SqliteCppWrapper::HasFitWithPartnerPrevious(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return lengthAuction >= 2 && Utils::GetSuitInt(bids.at(lengthAuction - 2)) == suit;
}

bool SqliteCppWrapper::HasFitWithPartnerFirst(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return lengthAuction >= 6 && Utils::GetSuitInt(bids.at(lengthAuction - 6)) == suit;
}

bool SqliteCppWrapper::IsReverse(int suit, int rank, int previousSuit, int previousRank)
{
    return (previousSuit <= 3 && previousSuit > suit && previousRank < rank);
}

bool SqliteCppWrapper::IsNonReverse(int suit, int rank, int previousSuit, int previousRank)
{
    return (previousSuit <= 3 && (previousSuit < suit || previousRank > rank));
}

bool SqliteCppWrapper::IsRebidOwnSuit(const std::vector<int>& bids, size_t lengthAuction, int suit)
{
    return (lengthAuction >= 4 && Utils::GetSuitInt(bids.at(lengthAuction - 4)) == suit);
}

std::tuple<int, std::string> SqliteCppWrapper::GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& board, const std::string& previousBidding)
{
    try
    {
        // Bind parameters
        queryShapeRelative->reset();
        queryShapeRelative->bind(":lastBidId", board.lastBidId);
        queryShapeRelative->bind(":keyCards", board.keyCards);
        queryShapeRelative->bind(":trumpQueen", board.trumpQueen);
        queryShapeRelative->bind(":previousBidding", previousBidding);
        queryShapeRelative->bind(":fitWithPartner", std::to_string(board.fitWithPartnerSuit));
        queryShapeRelative->bind(":spadeControl", hand.controls[0]);
        queryShapeRelative->bind(":heartControl", hand.controls[1]);
        queryShapeRelative->bind(":diamondControl", hand.controls[2]);
        queryShapeRelative->bind(":clubControl", hand.controls[3]);
        queryShapeRelative->bind(":allControlsPresent", board.allControlsPresent);
        queryShapeRelative->bind(":lastBid", Utils::GetBidASCII(board.lastBidId));
        queryShapeRelative->bind(":modules", modules);

        while (queryShapeRelative->executeStep())
        {
            auto bidId = queryShapeRelative->getColumn(0).getInt();
            auto str = queryShapeRelative->getColumn(1).getString();

            return std::make_tuple(bidId, str);
        }
        return std::make_tuple(0, "");
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

/// <summary>
/// Gets all the rules for this bid
/// </summary>
/// <returns>a JSON string with all the rules</returns>
std::string SqliteCppWrapper::GetRulesByBid(int bidId, const std::string& previousBidding)
{
    nlohmann::json j = GetInternalRulesByBid(bidId, previousBidding);
    std::stringstream ss;
    ss << j;
    auto s = ss.str();
    return s;
}

std::vector<std::unordered_map<std::string, std::string>> SqliteCppWrapper::GetInternalRulesByBid(int bidId, const std::string& previousBidding)
{
    auto bidIds = Utils::SplitAuction(previousBidding);
    auto position = (int)bidIds.size() + 1;
    auto isCompetitive = Utils::GetIsCompetitive(previousBidding);
    auto bidRank = Utils::GetRank(bidId);
    auto bidKindAuction = GetBidKindFromAuction(previousBidding, bidId);

    try
    {
        // Bind parameters
        queryRules->reset();
        queryRules->bind(":bidId", bidId);
        queryRules->bind(":modules", modules);
        queryRules->bind(":position", position);
        queryRules->bind(":isCompetitive", isCompetitive);
        queryRules->bind(":bidRank", bidRank);
        queryRules->bind(":bidKindAuction", (int)bidKindAuction);
        queryRules->bind(":previousBidding", previousBidding);

        std::vector<std::unordered_map<std::string, std::string>> records;

        while (queryRules->executeStep())
        {
            // Build map
            auto isAbsoluteRule = !queryRules->getColumn("BidId").isNull();
            auto isSuitBid = (bidId % 5 != 0) && bidId > 0;
            // TODO remove this strange check
            if (isAbsoluteRule || isSuitBid)
            {
                std::unordered_map<std::string, std::string> record;
                for (int i = 0; i < queryRules->getColumnCount() - 1; i++)
                {
                    auto column = queryRules->getColumn(i);
                    record.emplace(column.getName(), column.getString());
                }
                if (queryRules->getColumn("BidId").isNull())
                    UpdateMinMax(bidId, record);
                records.push_back(record);
            }
        }
        return records;
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

void SqliteCppWrapper::UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record) const
{
    auto suit = Utils::GetSuitFromBidId(bidId) + "s";
    switch ((BidKind)queryRules->getColumn("BidSuitKind").getInt())
    {
    case BidKind::FirstSuit:
    case BidKind::LowestSuit:
    case BidKind::HighestSuit:
    {
        record["Min" + suit] = queryRules->getColumn("MinFirstSuit").getString();
        record["Max" + suit] = queryRules->getColumn("MaxFirstSuit").getString();
    }
    break;
    case BidKind::SecondSuit:
    {
        record["Min" + suit] = queryRules->getColumn("MinSecondSuit").getString();
        record["Max" + suit] = queryRules->getColumn("MaxSecondSuit").getString();
    }
    break;
    case BidKind::PartnersSuit:
    {
        // TODO not technically correct, but it works
        record["Min" + suit] = "4";
        record["Max" + suit] = "13";
    }
    break;
    default:
        break;
    }
}

/// <summary>
/// Gets all the rules for this bid in the relativeRules table
/// </summary>
/// <returns>a JSON string with all the relative rules</returns>
std::string SqliteCppWrapper::GetRelativeRulesByBid(int bidId, const std::string& previousBidding)
{
    auto records = GetInternalRelativeRulesByBid(bidId, previousBidding);
    nlohmann::json j = records;
    std::stringstream ss;
    ss << j;
    auto s = ss.str(); 
    return s;
}

std::vector<std::unordered_map<std::string, std::string>> SqliteCppWrapper::GetInternalRelativeRulesByBid(int bidId, const std::string& previousBidding)
{
    try
    {
        // Bind parameters
        queryRelativeRules->reset();
        queryRelativeRules->bind(":bidId", bidId);
        queryRelativeRules->bind(":previousBidding", previousBidding);
        queryRelativeRules->bind(":lastBid", Utils::GetLastBidFromAuction(previousBidding));
        
        std::vector<std::unordered_map<std::string, std::string>> records;

        while (queryRelativeRules->executeStep())
        {
            std::unordered_map<std::string, std::string> record;
            for (int i = 0; i < queryRelativeRules->getColumnCount() - 1; i++)
            {
                auto column = queryRelativeRules->getColumn(i);
                record.emplace(column.getName(), column.getString());
            }
            records.push_back(record);
        }
        return records;
    }
    catch (const std::exception& e)
    {
        std::cerr << e.what();
        throw;
    }
}

void SqliteCppWrapper::SetModules(int modules)
{
    this->modules = modules;
}
