namespace Engine.DotNet;

public sealed class HandCharacteristic
{
    public string Hand { get; private set; } = string.Empty;

    public List<int> SuitLengths { get; private set; } = [];
    public bool IsBalanced { get; private set; }
    public bool IsSemiBalanced { get; private set; }
    public bool IsReverse { get; private set; }
    public int LengthFirstSuit { get; private set; }
    public int LengthSecondSuit { get; private set; }

    public int FirstSuit { get; private set; } = -1;
    public int SecondSuit { get; private set; } = -1;
    public int LowestSuit { get; private set; } = -1;
    public int HighestSuit { get; private set; } = -1;

    public int Hcp { get; private set; }
    public List<bool> Controls { get; private set; } = [];

    public HandCharacteristic(string hand) => Initialize(hand);

    private static bool GetHasControl(string suit)
    {
        return suit.Length <= 1 || Utils.NumberOfCards(suit, 'A') == 1 || Utils.NumberOfCards(suit, 'K') == 1;
    }

    public void Initialize(string hand)
    {
        Hand = hand;
        if (hand.Length != 16)
            throw new ArgumentException("Hand must have length 16.");

        var suits = hand.Split(',');
        if (suits.Length != 4)
            throw new ArgumentException("Hand must contain 4 suits.");

        SuitLengths = suits.Select(x => x.Length).ToList();
        var suitLengthSorted = SuitLengths.OrderByDescending(x => x).ToList();

        LengthFirstSuit = suitLengthSorted[0];
        LengthSecondSuit = suitLengthSorted[1];

        var suitsEqualLength = LengthFirstSuit == LengthSecondSuit;

        FirstSuit = suitsEqualLength ? -1 : SuitLengths.IndexOf(LengthFirstSuit);
        SecondSuit = suitsEqualLength ? -1 : SuitLengths.IndexOf(LengthSecondSuit);

        HighestSuit = !suitsEqualLength ? -1 : SuitLengths.IndexOf(LengthFirstSuit);
        LowestSuit = !suitsEqualLength ? -1 : SuitLengths.Count - 1 - SuitLengths.AsEnumerable().Reverse().ToList().IndexOf(LengthFirstSuit);

        Hcp = Utils.CalculateHcp(hand);

        Controls = [];
        for (var suit = 0; suit <= 3; suit++)
            Controls.Add(GetHasControl(suits[suit]));

        var ordered = suits.OrderByDescending(x => x.Length).ToList();
        var distribution = $"{ordered[0].Length}{ordered[1].Length}{ordered[2].Length}{ordered[3].Length}";

        IsBalanced = distribution is "4333" or "4432" or "5332";
        IsSemiBalanced = IsBalanced || distribution is "5422" or "4441" or "5431";
        IsReverse = FirstSuit > SecondSuit && !suitsEqualLength;
    }
}