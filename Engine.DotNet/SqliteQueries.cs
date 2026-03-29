namespace Engine.DotNet;

public static class SqliteQueries
{
    public const string ShapeSql = """
        WITH cte_rules AS (
            SELECT CASE 
                WHEN BidId IS NOT null THEN BidId
                WHEN BidId IS NULL AND BidSuitKind = 1 AND :firstSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :firstSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 2 AND :secondSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :secondSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 3 AND :lowestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :lowestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 4 AND :highestSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :highestSuit) + 1
                WHEN BidId IS NULL AND BidSuitKind = 5 AND :fitWithPartnerSuit >= 0 THEN (BidRank - 1) * 5 + (3 - :fitWithPartnerSuit) + 1
                ELSE 0
            END AS bidId,
            Description, Id, BidKindAuction, PreviousBidding, IsOpponentsSuit
            FROM Rules
            WHERE (bidId > :lastBidId OR bidId <= 0 OR bidId is NULL)
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
            ORDER BY Priority ASC
        )
        SELECT bidId, Description, Id
        FROM cte_rules 
        WHERE (bidId > :lastBidId OR bidId < 0)
          AND (BidKindAuction IS NULL OR getBidKindFromAuction(:previousBidding, bidId) = BidKindAuction)
          AND (IsOpponentsSuit IS NULL 
              OR (IsOpponentsSuit = 1 AND (4 - (bidId % 5)) = :opponentsSuit)
              OR (IsOpponentsSuit = 0 AND (4 - (bidId % 5)) <> :opponentsSuit))
        """;

    public const string RulesSql = """
        SELECT PreviousBidding, * FROM Rules 
        WHERE ((bidId = :bidId) OR (bidId is NULL AND :bidId > 0))
          AND (Module IS NULL OR :modules & Module = Module)
          AND Position = :position
          AND (IsCompetitive IS NULL OR IsCompetitive = :isCompetitive)
          AND (BidRank IS NULL OR BidRank = :bidRank)
          AND (BidKindAuction IS NULL OR BidKindAuction = :bidKindAuction)
          AND (PreviousBidding IS NULL OR regex_match(PreviousBidding, :previousBidding) = 1)
          AND (RelevantIds IS NULL OR :bidId LIKE '%' || RelevantIds || '%')
          AND UseInCalculation IS NULL
        """;

    public const string RelativeShapeSql = """
        SELECT bidId, Description FROM RelativeRules 
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
        ORDER BY Priority ASC
        """;

    public const string RelativeRulesSql = """
        SELECT * FROM RelativeRules 
        WHERE bidId = :bidId
          AND (PreviousBidding IS NULL or PreviousBidding = :previousBidding)
          AND (LastBid IS NULL or LastBid = :lastBid)
        """;
}