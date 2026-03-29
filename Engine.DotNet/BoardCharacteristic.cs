namespace Engine.DotNet;

public sealed class BoardCharacteristic
{
    public bool HasFit { get; private set; }
    public bool FitIsMajor { get; private set; }
    private List<int> PartnersSuits { get; }
    public int FitWithPartnerSuit { get; }
    public int OpponentsSuit { get; }
    public bool StopInOpponentsSuit { get; private set; }
    public int KeyCards { get; private set; }
    public bool TrumpQueen { get; private set; }
    public int Position { get; private set; }
    public int LastBidId { get; private set; }
    public bool IsCompetitive { get; private set; }
    public int MinHcpPartner { get; private set; }
    public bool AllControlsPresent { get; private set; }

    public BoardCharacteristic(HandCharacteristic hand, string previousBidding, InformationFromAuction informationFromAuction)
    {
        var bidIds = Utils.SplitAuction(previousBidding);
        Position = bidIds.Count + 1;

        PartnersSuits = informationFromAuction.PartnersSuits.ToList();
        OpponentsSuit = GetLongestSuit(informationFromAuction.OpenersSuits);
        StopInOpponentsSuit = GetHasStopInOpponentsSuit(hand.Hand, OpponentsSuit);

        var suitLengthCombined = hand.SuitLengths
            .Zip(PartnersSuits, (x, y) => x + y)
            .ToList();

        var firstSuitWithFitIndex = suitLengthCombined.FindIndex(x => x >= 8);
        FitWithPartnerSuit = firstSuitWithFitIndex == -1 ? -1 : firstSuitWithFitIndex;
        HasFit = firstSuitWithFitIndex != -1;
        FitIsMajor = suitLengthCombined[0] >= 8 || suitLengthCombined[1] >= 8;

        var suits = hand.Hand.Split(",");
        var trumpSuit = FitWithPartnerSuit == -1 ? string.Empty : suits[FitWithPartnerSuit];
        KeyCards = Utils.NumberOfCards(hand.Hand, 'A') + Utils.NumberOfCards(trumpSuit, 'K');
        TrumpQueen = Utils.NumberOfCards(trumpSuit, 'Q') > 0;
        LastBidId = Utils.GetLastBidIdFromAuction(previousBidding);

        IsCompetitive = Utils.GetIsCompetitive(previousBidding);
        MinHcpPartner = informationFromAuction.MinHcpPartner;
        AllControlsPresent = GetAllControlsPresent(hand, informationFromAuction, FitWithPartnerSuit);
    }

    private static int GetLongestSuit(List<int> suitLengths)
    {
        return suitLengths.Any(x => x > 0)
            ? suitLengths.IndexOf(suitLengths.Max())
            : -1;
    }

    private static bool GetHasStopInOpponentsSuit(string hand, int opponentsSuit)
    {
        if (opponentsSuit == -1)
            return false;

        var cardsInOpponentSuit = hand.Split(',')[opponentsSuit];
        if (cardsInOpponentSuit.Length == 0)
            return false;

        return cardsInOpponentSuit[0] switch
        {
            'A' => true,
            'K' => cardsInOpponentSuit.Length >= 2,
            'Q' => cardsInOpponentSuit.Length >= 3,
            'J' => cardsInOpponentSuit.Length >= 4,
            _ => false
        };
    }

    private static bool GetAllControlsPresent(HandCharacteristic handCharacteristic, InformationFromAuction informationFromAuction, int fitWithPartnerSuit)
    {
        var controlsCombined = informationFromAuction.Controls.ToList();
        for (var i = 0; i < 4; i++)
            controlsCombined[i] = controlsCombined[i] || handCharacteristic.Controls[i];

        for (var i = 0; i < 3; i++)
        {
            if (!controlsCombined[i] && i != fitWithPartnerSuit)
                return false;
        }

        return true;
    }
}