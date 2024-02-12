// ReSharper disable CppCStyleCast
#pragma once

#define SQLITECPP_COMPILE_DLL


#include "ISqliteWrapper.h"
#include "SQLiteCpp/SQLiteCpp.h"
#include <unordered_map>
#include "Api.h"

enum class BidKind;

class SqliteCppWrapper final : public ISqliteWrapper
{
    constexpr static std::string_view shapeSql = R"(WITH cte_rules AS (SELECT CASE 
                WHEN BidId IS NOT null THEN BidId
                WHEN BidId IS NULL AND BidSuitKind = 1 AND :firstSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :firstSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 2 AND :secondSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :secondSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 3 AND :lowestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :lowestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 4 AND :highestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :highestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 5 AND :fitWithPartnerSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :fitWithPartnerSuit) + 1
                ELSE 0
            END AS bidId,
            Description, Id, BidKindAuction, PreviousBidding, IsOpponentsSuit FROM Rules 
        WHERE (bidId > :lastBidId OR bidId <= 0 OR bidID is NULL)
        AND :minSpades BETWEEN MinSpades AND MaxSpades
        AND :minHearts BETWEEN MinHearts AND MaxHearts
        AND :minDiamonds BETWEEN MinDiamonds AND MaxDiamonds
        AND :minClubs BETWEEN MinClubs AND MaxClubs
        AND :minHcp BETWEEN MinHcp AND MaxHcp
        AND (IsBalanced IS NULL or IsBalanced = :isBalanced)
        AND (OpponentsSuit is NULL or OpponentsSuit = :opponentsSuit)
        AND (StopInOpponentsSuit is NULL or StopInOpponentsSuit = :stopInOpponentsSuit)
        AND :lengthFirstSuit BETWEEN MinFirstSuit AND MaxFirstSuit
        AND :lengthSecondSuit BETWEEN MinSecondSuit AND MaxSecondSuit
        AND (HasFit IS NULL or HasFit = :hasFit)
        AND (FitIsMajor IS NULL or FitIsMajor = :fitIsMajor)
        AND (Module IS NULL or :modules & Module = Module)
        AND Position = :position
        AND (IsCompetitive IS NULL or IsCompetitive = :isCompetitive)
        AND (IsReverse IS NULL or IsReverse = :isReverse)
        AND (IsSemiBalanced IS NULL or IsSemiBalanced = :isSemiBalanced)
        AND (PreviousBidding IS NULL OR regex_match(PreviousBidding, :previousBidding) = 1)
        ORDER BY Priority ASC)

        select bidId, Description, Id from cte_rules 
        WHERE (bidId > :lastBidId OR bidId < 0)
        AND (BidKindAuction IS NULL OR getBidKindFromAuction(:previousBidding, bidId) = BidKindAuction)
        AND (IsOpponentsSuit IS NULL 
            OR (IsOpponentsSuit = 1 AND (4 - (bidId % 5)) = :opponentsSuit)
            OR (IsOpponentsSuit = 0 AND (4 - (bidId % 5)) <> :opponentsSuit)))";

    constexpr static std::string_view rulesSql = R"(SELECT PreviousBidding, * FROM Rules 
        WHERE ((bidId = :bidId) OR (bidId is NULL AND :bidId > 0))
        AND (Module IS NULL OR :modules & Module = Module)
        AND Position = :position
        AND (IsCompetitive IS NULL OR IsCompetitive = :isCompetitive)
        AND (BidRank IS NULL OR BidRank = :bidRank)
        AND (BidKindAuction IS NULL OR BidKindAuction = :bidKindAuction)
        AND (PreviousBidding IS NULL OR regex_match(PreviousBidding, :previousBidding) = 1)
        AND (RelevantIds IS NULL OR :bidID LIKE '%' || RelevantIds || '%')
        AND UseInCalculation IS NULL)";

    constexpr static std::string_view relativeShapeSql = R"(SELECT bidId, Description FROM RelativeRules 
        WHERE bidId > :lastBidId
        AND (KeyCards IS NULL or KeyCards = :keyCards)
        AND (TrumpQueen IS NULL or TrumpQueen = :trumpQueen)
        AND (PreviousBidding IS NULL or PreviousBidding = :previousBidding)
        AND (TrumpSuits IS NULL OR TrumpSuits LIKE '%' || :fitWithPartner || '%')
        AND (SpadeControl IS NULL or SpadeControl = :spadeControl)
        AND (HeartControl IS NULL or HeartControl = :heartControl)
        AND (DiamondControl IS NULL or DiamondControl = :diamondControl)
        AND (ClubControl IS NULL or ClubControl = :clubControl)
        AND (AllControlsPresent IS NULL or AllControlsPresent = :allControlsPresent)
        AND (LastBid IS NULL or LastBid = :lastBid)
        AND (Module IS NULL or :modules & Module = Module)
        ORDER BY Priority ASC)";

    constexpr static std::string_view relativeRulesSql = R"(SELECT * FROM RelativeRules 
        WHERE bidId = :bidId
        AND (PreviousBidding IS NULL or PreviousBidding = :previousBidding)
        AND (LastBid IS NULL or LastBid = :lastBid))";

    std::unique_ptr<SQLite::Database> db;
    std::unique_ptr<SQLite::Statement> queryShape;
    std::unique_ptr<SQLite::Statement> queryRules;
    std::unique_ptr<SQLite::Statement> queryShapeRelative;
    std::unique_ptr<SQLite::Statement> queryRelativeRules;

    int modules = (int)Modules::FiveCardMajors;

public:
    explicit SqliteCppWrapper(const std::string& database);
    static BidKindAuction GetBidKindFromAuction(const std::string& previousBidding, int bidId);
private:
    std::tuple<int, std::string> GetRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, const std::string& previousBidding) override;
    std::tuple<int, std::string> GetRelativeRule(const HandCharacteristic& hand, const BoardCharacteristic& boardCharacteristic, const std::string& previousBidding) override;
    std::string GetRulesByBid(int bidId, const std::string& previousBidding) override;
    std::vector<std::unordered_map<std::string, std::string>> GetInternalRulesByBid(int bidId, const std::string& previousBidding) override;
    static bool HasFitWithPartnerPrevious(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartnerFirst(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool HasFitWithPartner(const std::vector<int>& bids, size_t lengthAuction, int suit);
    static bool IsReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsNonReverse(int suit, int rank, int previousSuit, int previousRank);
    static bool IsRebidOwnSuit(const std::vector<int>& bids, size_t lengthAuction, int suit);
    void UpdateMinMax(int bidId, std::unordered_map<std::string, std::string>& record) const;
    std::string GetRelativeRulesByBid(int bidId, const std::string& previousBidding) override;
    std::vector<std::unordered_map<std::string, std::string>> GetInternalRelativeRulesByBid(int bidId, const std::string& previousBidding) override;
    void SetModules(int modules) override;
};