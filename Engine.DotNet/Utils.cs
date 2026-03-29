namespace Engine.DotNet;

public static class Utils
{
    public static string GetSuitFromBidId(int bidId) => GetSuit(GetSuitInt(bidId));

    public static string GetSuit(int suit) => suit switch
    {
        0 => "Spade",
        1 => "Heart",
        2 => "Diamond",
        3 => "Club",
        _ => throw new ArgumentException("Unknown suit")
    };

    public static int GetSuitInt(int bidId) => 4 - (bidId % 5);

    private static int GetSuit(string suit)
    {
        return suit switch
        {
            "NT" => -1,
            "S" => 0,
            "H" => 1,
            "D" => 2,
            "C" => 3,
            _ => throw new ArgumentException("Unknown suit")
        };
    }

    private static string GetSuitASCII(int bidId) => GetSuitInt(bidId) switch
    {
        0 => "S",
        1 => "H",
        2 => "D",
        3 => "C",
        4 => "NT",
        _ => throw new ArgumentException("Unknown suit")
    };

    public static List<int> SplitAuction(string auction)
    {
        var ret = new List<int>();
        var currentBid = string.Empty;

        foreach (var c in auction)
        {
            if (currentBid.Length > 0 && (char.IsDigit(c) || c == 'P' || c == 'X'))
            {
                ret.Add(GetBidId(currentBid));
                currentBid = string.Empty;
            }

            currentBid += c;
        }

        if (currentBid.Length > 0)
            ret.Add(GetBidId(currentBid));

        return ret;
    }

    private static int GetBidId(string bid)
    {
        if (bid == "Pass")
            return 0;
        if (bid == "X")
            return -1;
        if (bid == "XX")
            return -2;

        var rank = int.Parse(bid[..1]);
        var suit = GetSuit(bid[1..]);
        return (rank - 1) * 5 + 4 - suit;
    }

    public static int GetRank(int bidId) => (bidId - 1) / 5 + 1;

    public static int NumberOfCards(string hand, char card) => hand.Count(c => c == card);

    public static int CalculateHcp(string hand)
    {
        var aces = NumberOfCards(hand, 'A');
        var kings = NumberOfCards(hand, 'K');
        var queens = NumberOfCards(hand, 'Q');
        var jacks = NumberOfCards(hand, 'J');
        return aces * 4 + kings * 3 + queens * 2 + jacks;
    }

    public static bool GetIsCompetitive(string bidding)
    {
        var bidIds = SplitAuction(bidding);

        var bidsOpenerTeam = new List<int>();
        var bidsOtherTeam = new List<int>();

        var isOpenerTeam = true;
        foreach (var bidId in bidIds)
        {
            if (isOpenerTeam)
                bidsOpenerTeam.Add(bidId);
            else
                bidsOtherTeam.Add(bidId);

            isOpenerTeam = !isOpenerTeam;
        }

        return bidsOpenerTeam.Any(b => b > 0) && bidsOtherTeam.Any(b => b > 0);
    }

    public static string GetBidASCII(int bidId)
    {
        return bidId switch
        {
            0 => "Pass",
            -1 => "X",
            _ => GetRank(bidId) + GetSuitASCII(bidId)
        };
    }

    public static int GetLastBidIdFromAuction(string bidding)
    {
        var bidIds = SplitAuction(bidding);
        var lastBidding = bidIds.AsEnumerable().Reverse().FirstOrDefault(bidId => bidId > 0);
        return lastBidding;
    }

    public static string GetLastBidFromAuction(string bidding) => GetBidASCII(GetLastBidIdFromAuction(bidding));
}