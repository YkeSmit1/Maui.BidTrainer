using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Engine.DotNet;

public sealed class SqliteCppWrapper : ISqliteWrapper
{
    private readonly SqliteConnection db;
    private int _modules;

    public SqliteCppWrapper(string database)
    {
        db = new SqliteConnection($"Data Source={database}");
        db.Open();

        RegisterFunctions();
    }

    private void RegisterFunctions()
    {
        db.CreateFunction<string?, string?, int?>("regex_match", (regexp, text) =>
        {
            if (string.IsNullOrWhiteSpace(regexp) || string.IsNullOrWhiteSpace(text))
                return null;

            return Regex.IsMatch(text, regexp) ? 1 : 0;
        });

        db.CreateFunction<string?, int, int?>("getBidKindFromAuction", (previousBidding, bidId) =>
        {
            if (string.IsNullOrWhiteSpace(previousBidding))
                return null;

            return (int)GetBidKindFromAuction(previousBidding, bidId);
        });
    }

    public (int bidId, string description) GetRule(
        HandCharacteristic handCharacteristic,
        BoardCharacteristic boardCharacteristic,
        string previousBidding)
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = GetShapeSql();

        BindShapeParameters(cmd, handCharacteristic, boardCharacteristic, previousBidding);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (reader.GetInt32(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1));

        return (0, string.Empty);
    }

    public (int bidId, string description) GetRelativeRule(
        HandCharacteristic handCharacteristic,
        BoardCharacteristic boardCharacteristic,
        string previousSlamBidding)
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = GetRelativeShapeSql();

        BindRelativeShapeParameters(cmd, handCharacteristic, boardCharacteristic, previousSlamBidding);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (reader.GetInt32(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1));

        return (0, string.Empty);
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

        using var cmd = db.CreateCommand();
        cmd.CommandText = GetRulesSql();

        cmd.Parameters.AddWithValue(":bidId", bidId);
        cmd.Parameters.AddWithValue(":modules", _modules);
        cmd.Parameters.AddWithValue(":position", position);
        cmd.Parameters.AddWithValue(":isCompetitive", isCompetitive ? 1 : 0);
        cmd.Parameters.AddWithValue(":bidRank", bidRank);
        cmd.Parameters.AddWithValue(":bidKindAuction", (int)bidKindAuction);
        cmd.Parameters.AddWithValue(":previousBidding", previousBidding);

        var records = new List<Dictionary<string, string>>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var isAbsoluteRule = !reader.IsDBNull(reader.GetOrdinal("BidId"));
            var isSuitBid = (bidId % 5 != 0) && bidId > 0;

            if (isAbsoluteRule || isSuitBid)
            {
                var record = ReadRecord(reader);

                if (reader.IsDBNull(reader.GetOrdinal("BidId")))
                    UpdateMinMax(bidId, record, reader);

                records.Add(record);
            }
        }

        return records;
    }

    public List<Dictionary<string, string>> GetInternalRelativeRulesByBid(int bidId, string previousBidding)
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = GetRelativeRulesSql();

        cmd.Parameters.AddWithValue(":bidId", bidId);
        cmd.Parameters.AddWithValue(":previousBidding", previousBidding);
        cmd.Parameters.AddWithValue(":lastBid", Utils.GetLastBidFromAuction(previousBidding));

        var records = new List<Dictionary<string, string>>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            records.Add(ReadRecord(reader));
        }

        return records;
    }

    private void BindShapeParameters(
        SqliteCommand cmd,
        HandCharacteristic hand,
        BoardCharacteristic board,
        string previousBidding)
    {
        cmd.Parameters.AddWithValue(":firstSuit", hand.FirstSuit);
        cmd.Parameters.AddWithValue(":secondSuit", hand.SecondSuit);
        cmd.Parameters.AddWithValue(":lowestSuit", hand.LowestSuit);
        cmd.Parameters.AddWithValue(":highestSuit", hand.HighestSuit);
        cmd.Parameters.AddWithValue(":fitWithPartnerSuit", board.FitWithPartnerSuit);

        cmd.Parameters.AddWithValue(":lastBidId", board.LastBidId);
        cmd.Parameters.AddWithValue(":minSpades", hand.SuitLengths[0]);
        cmd.Parameters.AddWithValue(":minHearts", hand.SuitLengths[1]);
        cmd.Parameters.AddWithValue(":minDiamonds", hand.SuitLengths[2]);
        cmd.Parameters.AddWithValue(":minClubs", hand.SuitLengths[3]);

        cmd.Parameters.AddWithValue(":minHcp", hand.Hcp);
        cmd.Parameters.AddWithValue(":isBalanced", hand.IsBalanced ? 1 : 0);
        cmd.Parameters.AddWithValue(":opponentsSuit", board.OpponentsSuit);
        cmd.Parameters.AddWithValue(":stopInOpponentsSuit", board.StopInOpponentsSuit ? 1 : 0);

        cmd.Parameters.AddWithValue(":lengthFirstSuit", hand.LengthFirstSuit);
        cmd.Parameters.AddWithValue(":lengthSecondSuit", hand.LengthSecondSuit);
        cmd.Parameters.AddWithValue(":hasFit", board.HasFit ? 1 : 0);
        cmd.Parameters.AddWithValue(":fitIsMajor", board.FitIsMajor ? 1 : 0);
        cmd.Parameters.AddWithValue(":modules", _modules);
        cmd.Parameters.AddWithValue(":position", board.Position);
        cmd.Parameters.AddWithValue(":isCompetitive", board.IsCompetitive ? 1 : 0);
        cmd.Parameters.AddWithValue(":isReverse", hand.IsReverse ? 1 : 0);
        cmd.Parameters.AddWithValue(":isSemiBalanced", hand.IsSemiBalanced ? 1 : 0);
        cmd.Parameters.AddWithValue(":previousBidding", previousBidding);
    }

    private void BindRelativeShapeParameters(
        SqliteCommand cmd,
        HandCharacteristic hand,
        BoardCharacteristic board,
        string previousSlamBidding)
    {
        cmd.Parameters.AddWithValue(":lastBidId", board.LastBidId);
        cmd.Parameters.AddWithValue(":keyCards", board.KeyCards);
        cmd.Parameters.AddWithValue(":trumpQueen", board.TrumpQueen ? 1 : 0);
        cmd.Parameters.AddWithValue(":previousBidding", previousSlamBidding);
        cmd.Parameters.AddWithValue(":fitWithPartner", board.FitWithPartnerSuit.ToString());
        cmd.Parameters.AddWithValue(":spadeControl", hand.Controls[0] ? 1 : 0);
        cmd.Parameters.AddWithValue(":heartControl", hand.Controls[1] ? 1 : 0);
        cmd.Parameters.AddWithValue(":diamondControl", hand.Controls[2] ? 1 : 0);
        cmd.Parameters.AddWithValue(":clubControl", hand.Controls[3] ? 1 : 0);
        cmd.Parameters.AddWithValue(":allControlsPresent", board.AllControlsPresent ? 1 : 0);
        cmd.Parameters.AddWithValue(":lastBid", Utils.GetBidASCII(board.LastBidId));
        cmd.Parameters.AddWithValue(":modules", _modules);
    }

    private static Dictionary<string, string> ReadRecord(SqliteDataReader reader)
    {
        var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            record[name] = reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString() ?? string.Empty;
        }

        return record;
    }

    private static void UpdateMinMax(int bidId, Dictionary<string, string> record, SqliteDataReader reader)
    {
        var suit = Utils.GetSuitFromBidId(bidId) + "s";
        var bidSuitKind = reader.GetInt32(reader.GetOrdinal("BidSuitKind"));

        switch ((BidKind)bidSuitKind)
        {
            case BidKind.FirstSuit:
            case BidKind.LowestSuit:
            case BidKind.HighestSuit:
                record["Min" + suit] = reader.GetString(reader.GetOrdinal("MinFirstSuit"));
                record["Max" + suit] = reader.GetString(reader.GetOrdinal("MaxFirstSuit"));
                break;

            case BidKind.SecondSuit:
                record["Min" + suit] = reader.GetString(reader.GetOrdinal("MinSecondSuit"));
                record["Max" + suit] = reader.GetString(reader.GetOrdinal("MaxSecondSuit"));
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