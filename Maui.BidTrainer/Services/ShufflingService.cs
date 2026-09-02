using Common;

namespace Maui.BidTrainer.Services;

public class ShufflingService
{
    private record Card(Face Rank, Suit Suit);
    
    public Dictionary<Player, string> Deal(Random random)
    {
        var deck = Enum.GetValues<Suit>().Except([Suit.NoTrump])
            .SelectMany(suit => Enum.GetValues<Face>()
                .Select(rank => new Card(rank, suit)))
            .ToArray();

        // Fisher-Yates shuffle
        for (var i = deck.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        // Deal 13 cards to each player
        var players = Enum.GetValues<Player>();
        return players.ToDictionary(
            player => player,
            player => ToHandString(deck.Skip((int)player * 13).Take(13))
        );
    }
    
    private static string ToHandString(IEnumerable<Card> hand)
    {
        const string ranks = "AKQJT98765432";
        return string.Join(",", Enum.GetValues<Suit>().Except([Suit.NoTrump])
            .Select(suit => string.Concat(
                hand.Where(c => c.Suit == suit)
                    .OrderBy(c => ranks.IndexOf(Util.GetFaceDescription(c.Rank)))
                    .Select(c => Util.GetFaceDescription(c.Rank)))));
    }    
}