namespace Engine.DotNet;

public static class Api
{
    private static ISqliteWrapper? sqliteWrapper;

    private static HandCharacteristic GetHandCharacteristic(string hand)
    {
        return new HandCharacteristic(hand);
    }

    private static ISqliteWrapper GetSqliteWrapper()
    {
        return sqliteWrapper ?? throw new InvalidOperationException("Setup was not called to initialize sqlite database");
    }

    public static int GetBidFromRule(string hand, string previousBidding, out string description)
    {
        var handCharacteristic = GetHandCharacteristic(hand);
        var informationFromAuction = new InformationFromAuction(GetSqliteWrapper(), previousBidding);
        var boardCharacteristic = new BoardCharacteristic(handCharacteristic, previousBidding, informationFromAuction);

        var isSlamBidding = informationFromAuction.IsSlamBidding ||
                            (handCharacteristic.Hcp + boardCharacteristic.MinHcpPartner >= 29 && boardCharacteristic.HasFit);

        var result = !isSlamBidding
            ? GetSqliteWrapper().GetRule(handCharacteristic, boardCharacteristic, previousBidding)
            : GetSqliteWrapper().GetRelativeRule(handCharacteristic, boardCharacteristic, informationFromAuction.PreviousSlamBidding);

        description = result.description ?? "";
        return result.bidId;
    }

    public static void Setup(string database)
    {
        sqliteWrapper = new SqliteCppWrapper(database);
    }

    public static string GetRulesByBid(int bidId, string previousBidding)
    {
        var informationFromAuction = new InformationFromAuction(GetSqliteWrapper(), previousBidding);
        return informationFromAuction.IsSlamBidding
            ? GetSqliteWrapper().GetRelativeRulesByBid(bidId, informationFromAuction.PreviousSlamBidding)
            : GetSqliteWrapper().GetRulesByBid(bidId, previousBidding);
    }

    public static void SetModules(int modules)
    {
        GetSqliteWrapper().SetModules(modules);
    }

    public static string GetInformationFromAuction(string previousBidding)
    {
        var informationFromAuction = new InformationFromAuction(GetSqliteWrapper(), previousBidding);
        return informationFromAuction.AsJson();
    }
}