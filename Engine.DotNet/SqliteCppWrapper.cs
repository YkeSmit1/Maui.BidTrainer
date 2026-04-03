using System.Text.Json;
using System.Text.RegularExpressions;
using SQLite;
using SQLitePCL;

namespace Engine.DotNet;

public sealed partial class SqliteCppWrapper : ISqliteWrapper
{
    private readonly SQLiteConnection db;
    private int _modules;

    public SqliteCppWrapper(string database)
    {
        db = new SQLiteConnection(database);

        RegisterFunctions();
    }
    
    private void RegisterFunctions()
    {
        raw.sqlite3_create_function(db.Handle, "regex_match", 2, null, RegexMatchSqlite);
        raw.sqlite3_create_function(db.Handle, "getBidKindFromAuction", 2, null, BidKindFromAuctionSqlite);
        return;

        void RegexMatchSqlite(sqlite3_context ctx, object userData, sqlite3_value[] args)
        {
            var regexp = raw.sqlite3_value_text(args[0]).utf8_to_string();
            var text = raw.sqlite3_value_text(args[1]).utf8_to_string();
            raw.sqlite3_result_int(ctx, RegexMatch(regexp, text));
        }

        int RegexMatch(string regexp, string text)
        {
            if (string.IsNullOrWhiteSpace(regexp) || string.IsNullOrWhiteSpace(text))
                return 0;

            return Regex.IsMatch(text, regexp) ? 1 : 0;
        }

        void BidKindFromAuctionSqlite(sqlite3_context ctx, object userData, sqlite3_value[] args)
        {
            var previousBidding = raw.sqlite3_value_text(args[0]).utf8_to_string();
            var bidId = raw.sqlite3_value_int(args[1]);
            raw.sqlite3_result_int(ctx, BidKindFromAuction(previousBidding, bidId));
        }
        

        int BidKindFromAuction(string previousBidding, int bidId)
        {
            if (string.IsNullOrWhiteSpace(previousBidding))
                return 0;

            return (int)GetBidKindFromAuction(previousBidding, bidId);
        }
    }

    public (int bidId, string? description) GetRule(
        HandCharacteristic handCharacteristic,
        BoardCharacteristic boardCharacteristic,
        string previousBidding)
    {
        var cmd = db.CreateCommand(GetShapeSql(), BuildShapeParameters(handCharacteristic, boardCharacteristic, previousBidding));

        return cmd.ExecuteQuery<(int, string)>().FirstOrDefault();
    }

    public (int bidId, string? description) GetRelativeRule(
        HandCharacteristic handCharacteristic,
        BoardCharacteristic boardCharacteristic,
        string previousSlamBidding)
    {
        var cmd = db.CreateCommand(GetRelativeShapeSql(), BuildRelativeShapeParameters(handCharacteristic, boardCharacteristic, previousSlamBidding));

        return cmd.ExecuteQuery<(int, string)>().FirstOrDefault();
    }
    
    public string GetRulesByBid(int bidId, string previousBidding)
    {
        var records = GetInternalRulesByBid(bidId, previousBidding);
        return JsonSerializer.Serialize(records);
    }

    public string GetRelativeRulesByBid(int bidId, string previousSlamBidding)
    {
        var records = GetInternalRelativeRulesByBid(bidId, previousSlamBidding);
        return JsonSerializer.Serialize(records);
    }

    public void SetModules(int modules)
    {
        this._modules = modules;
    }

    public List<Dictionary<string, string>> GetInternalRulesByBid(int bidId, string previousBidding)
    {
        var bidIds = Utils.SplitAuction(previousBidding);
        var position = bidIds.Count + 1;
        var isCompetitive = Utils.GetIsCompetitive(previousBidding);
        var bidRank = Utils.GetRank(bidId);
        var bidKindAuction = GetBidKindFromAuction(previousBidding, bidId);

        var cmd = db.CreateCommand(GetRulesSql(), new Dictionary<string, object>
        {
            [":bidId"] = bidId,
            [":modules"] = _modules,
            [":position"] = position,
            [":isCompetitive"] = isCompetitive ? 1 : 0,
            [":bidRank"] = bidRank,
            [":bidKindAuction"] = (int)bidKindAuction,
            [":previousBidding"] = previousBidding
        });

        var rows = cmd.ExecuteQuery<RuleRow>();

        var records = new List<Dictionary<string, string>>();

        foreach (var row in rows)
        {
            var isAbsoluteRule = row.BidId.HasValue;
            var isSuitBid = (bidId % 5 != 0) && bidId > 0;

            if (isAbsoluteRule || isSuitBid)
            {
                var record = row.ToDictionary();

                if (!row.BidId.HasValue)
                    UpdateMinMax(bidId, record, row);

                records.Add(record);
            }
        }

        return records;
    }

    public List<Dictionary<string, string>> GetInternalRelativeRulesByBid(int bidId, string previousBidding)
    {
        var cmd = db.CreateCommand(GetRelativeRulesSql(), new Dictionary<string, object>
        {
            [":bidId"] = bidId,
            [":previousBidding"] = previousBidding,
            [":lastBid"] = Utils.GetLastBidFromAuction(previousBidding)
        });
        
        var rows = cmd.ExecuteQuery<RelativeRuleRow>();

        return rows.Select(row => row.ToDictionary()).ToList();
    }

    private Dictionary<string, object> BuildShapeParameters(
        HandCharacteristic hand,
        BoardCharacteristic board,
        string previousBidding)
        => new()
        {
            [":firstSuit"] = hand.FirstSuit,
            [":secondSuit"] = hand.SecondSuit,
            [":lowestSuit"] = hand.LowestSuit,
            [":highestSuit"] = hand.HighestSuit,
            [":fitWithPartnerSuit"] = board.FitWithPartnerSuit,
            
            [":lastBidId"] = board.LastBidId,
            [":minSpades"] = hand.SuitLengths[0],
            [":minHearts"] = hand.SuitLengths[1],
            [":minDiamonds"] = hand.SuitLengths[2],
            [":minClubs"] = hand.SuitLengths[3],
            
            [":minHcp"] = hand.Hcp,
            [":isBalanced"] = hand.IsBalanced ? 1 : 0,
            [":opponentsSuit"] = board.OpponentsSuit,
            [":stopInOpponentsSuit"] = board.StopInOpponentsSuit ? 1 : 0,
            
            [":lengthFirstSuit"] = hand.LengthFirstSuit,
            [":lengthSecondSuit"] = hand.LengthSecondSuit,
            [":hasFit"] = board.HasFit ? 1 : 0,
            [":fitIsMajor"] = board.FitIsMajor ? 1 : 0,
            [":modules"] = _modules,
            [":position"] = board.Position,
            [":isCompetitive"] = board.IsCompetitive ? 1 : 0,
            [":isReverse"] = hand.IsReverse ? 1 : 0,
            [":isSemiBalanced"] = hand.IsSemiBalanced ? 1 : 0,
            [":previousBidding"] = previousBidding
        };

    private Dictionary<string, object> BuildRelativeShapeParameters(
        HandCharacteristic hand,
        BoardCharacteristic board,
        string previousSlamBidding)
        => new()
        {
            [":lastBidId"] = board.LastBidId,
            [":keyCards"] = board.KeyCards,
            [":trumpQueen"] = board.TrumpQueen ? 1 : 0,
            [":previousBidding"] = previousSlamBidding,
            [":fitWithPartner"] = board.FitWithPartnerSuit.ToString(),
            [":spadeControl"] = hand.Controls[0] ? 1 : 0,
            [":heartControl"] = hand.Controls[1] ? 1 : 0,
            [":diamondControl"] = hand.Controls[2] ? 1 : 0,
            [":clubControl"] = hand.Controls[3] ? 1 : 0,
            [":allControlsPresent"] = board.AllControlsPresent ? 1 : 0,
            [":lastBid"] = Utils.GetBidASCII(board.LastBidId),
            [":modules"] = _modules
        };


    private static void UpdateMinMax(int bidId, Dictionary<string, string> record, RuleRow row)
    {
        var suit = Utils.GetSuitFromBidId(bidId) + "s";
        var bidSuitKind = row.BidSuitKind ?? 0;

        switch ((BidKind)bidSuitKind)
        {
            case BidKind.FirstSuit:
            case BidKind.LowestSuit:
            case BidKind.HighestSuit:
                record["Min" + suit] = row.MinFirstSuit ?? string.Empty;
                record["Max" + suit] = row.MaxFirstSuit ?? string.Empty;
                break;

            case BidKind.SecondSuit:
                record["Min" + suit] = row.MinSecondSuit ?? string.Empty;
                record["Max" + suit] = row.MaxSecondSuit ?? string.Empty;
                break;

            case BidKind.PartnersSuit:
                record["Min" + suit] = "4";
                record["Max" + suit] = "13";
                break;
        }
    }

    public static BidKindAuction GetBidKindFromAuction(string previousBidding, int bidId)
    {
        var bids = Utils.SplitAuction(previousBidding);
        var lengthAuction = bids.Count;

        var suit = Utils.GetSuitInt(bidId);
        if (HasFitWithPartner(bids, lengthAuction, suit))
            return BidKindAuction.PartnersSuit;

        if (IsRebidOwnSuit(bids, lengthAuction, suit))
            return BidKindAuction.OwnSuit;

        if (lengthAuction < 4 || suit is < 0 or > 3) 
            return BidKindAuction.UnknownSuit;
        var rank = Utils.GetRank(bidId);
        var previousSuit = Utils.GetSuitInt(bids[lengthAuction - 4]);
        var previousRank = Utils.GetRank(bids[lengthAuction - 4]);

        if (IsNonReverse(suit, rank, previousSuit, previousRank))
            return BidKindAuction.NonReverse;

        return IsReverse(suit, rank, previousSuit, previousRank) ? BidKindAuction.Reverse : BidKindAuction.UnknownSuit;
    }

    private static bool HasFitWithPartner(List<int> bids, int lengthAuction, int suit)
        => HasFitWithPartnerPrevious(bids, lengthAuction, suit) || HasFitWithPartnerFirst(bids, lengthAuction, suit);

    private static bool HasFitWithPartnerPrevious(List<int> bids, int lengthAuction, int suit)
        => lengthAuction >= 2 && Utils.GetSuitInt(bids[lengthAuction - 2]) == suit;

    private static bool HasFitWithPartnerFirst(List<int> bids, int lengthAuction, int suit)
        => lengthAuction >= 6 && Utils.GetSuitInt(bids[lengthAuction - 6]) == suit;

    private static bool IsReverse(int suit, int rank, int previousSuit, int previousRank)
        => previousSuit <= 3 && previousSuit > suit && previousRank < rank;

    private static bool IsNonReverse(int suit, int rank, int previousSuit, int previousRank)
        => previousSuit <= 3 && (previousSuit < suit || previousRank > rank);

    private static bool IsRebidOwnSuit(List<int> bids, int lengthAuction, int suit)
        => lengthAuction >= 4 && Utils.GetSuitInt(bids[lengthAuction - 4]) == suit;

    private static string GetShapeSql() => SqliteQueries.ShapeSql;
    private static string GetRulesSql() => SqliteQueries.RulesSql;
    private static string GetRelativeShapeSql() => SqliteQueries.RelativeShapeSql;
    private static string GetRelativeRulesSql() => SqliteQueries.RelativeRulesSql;
}

public enum BidKind
{
    UnknownSuit,
    FirstSuit,
    SecondSuit,
    LowestSuit,
    HighestSuit,
    PartnersSuit
}

public enum BidKindAuction
{
    UnknownSuit,
    NonReverse,
    Reverse,
    OwnSuit,
    PartnersSuit
}