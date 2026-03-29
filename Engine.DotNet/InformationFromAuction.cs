using System.Text.Json;

namespace Engine.DotNet;

public sealed class InformationFromAuction
{
    public List<int> PartnersSuits { get;  init; } = [0, 0, 0, 0];
    public List<int> OpenersSuits { get;  init; } = [0, 0, 0, 0];
    public int MinHcpPartner { get; }

    public List<bool> Controls { get; } = [false, false, false, false];
    public string PreviousSlamBidding { get; private set; } = string.Empty;
    public bool IsSlamBidding { get; }
    private int KeyCardsPartner { get; set; }
    private bool TrumpQueenPartner { get; set; }

    public InformationFromAuction() { }

    public InformationFromAuction(ISqliteWrapper sqliteWrapper, string previousBidding)
    {
        var minSuitLengths = new List<List<int>>
        {
            new() { 0, 0, 0, 0 },
            new() { 0, 0, 0, 0 },
            new() { 0, 0, 0, 0 },
            new() { 0, 0, 0, 0 }
        };

        var bidIds = Utils.SplitAuction(previousBidding);
        var position = 1;
        var currentBidding = string.Empty;
        var partner = (bidIds.Count + 2) % 4;

        foreach (var bidId in bidIds)
        {
            if (bidId != 0)
            {
                var player = (position - 1) % 4;
                var isPartner = player == partner;

                if (!IsSlamBidding)
                {
                    var rules = sqliteWrapper.GetInternalRulesByBid(bidId, currentBidding);
                    if (rules.Count > 0)
                    {
                        for (var i = 0; i < 4; i++)
                            minSuitLengths[player][i] = Math.Max(minSuitLengths[player][i], GetLowestValue(rules, "Min" + Utils.GetSuit(i) + "s"));

                        if (isPartner)
                            MinHcpPartner = Math.Max(MinHcpPartner, GetLowestValue(rules, "MinHcp"));
                    }
                    else
                    {
                        if (ExtraInfoFromRelativeRules(sqliteWrapper, bidId, string.Empty, isPartner))
                        {
                            var trumpSuit = Utils.GetSuitInt(bidIds[position - 3]);
                            if (isPartner)
                                minSuitLengths[player][trumpSuit] = Math.Max(minSuitLengths[player][trumpSuit], 4);

                            IsSlamBidding = true;
                            currentBidding = string.Empty;
                        }
                    }
                }
                else
                {
                    ExtraInfoFromRelativeRules(sqliteWrapper, bidId, currentBidding, isPartner);
                }
            }

            currentBidding += Utils.GetBidASCII(bidId);
            position++;
        }

        if (bidIds.Count % 2 != 0)
            IsSlamBidding = false;

        if (IsSlamBidding)
            PreviousSlamBidding = currentBidding;

        PartnersSuits = minSuitLengths[partner];
        OpenersSuits = minSuitLengths[0];
    }

    private bool ExtraInfoFromRelativeRules(ISqliteWrapper sqliteWrapper, int bidId, string currentBidding, bool isPartner)
    {
        var rules = sqliteWrapper.GetInternalRelativeRulesByBid(bidId, currentBidding);
        if (rules.Count <= 0) 
            return false;
        if (!isPartner) 
            return true;
        for (var i = 0; i < 4; i++)
            Controls[i] = Controls[i] || AllTrue(rules, Utils.GetSuit(i) + "Control");

        KeyCardsPartner = Math.Max(KeyCardsPartner, GetLowestValue(rules, "KeyCards"));
        TrumpQueenPartner = TrumpQueenPartner || AllTrue(rules, "TrumpQueen");

        return true;

    }

    private static int GetLowestValue(List<Dictionary<string, string>> rules, string columnName)
    {
        if (rules.Count == 0)
            return 0;

        if (rules.All(a => a[columnName] == string.Empty))
            return 0;

        var minElement = rules.OrderBy(a => string.IsNullOrEmpty(a[columnName]) ? int.MaxValue : int.Parse(a[columnName])).First();
        var value = minElement[columnName];
        return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
    }

    private static bool AllTrue(List<Dictionary<string, string>> rules, string columnName)
    {
        if (rules.Count == 0)
            return false;

        return rules.All(a => a[columnName] != string.Empty && int.Parse(a[columnName]) == 1);
    }

    public string AsJson()
    {
        var payload = new
        {
            minSuitLengthsPartner = PartnersSuits,
            minHcpPartner = MinHcpPartner,
            controls = Controls,
            keyCardsPartner = KeyCardsPartner,
            trumpQueenPartner = TrumpQueenPartner
        };

        return JsonSerializer.Serialize(payload);
    }
}