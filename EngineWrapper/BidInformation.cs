using Common;
using System.Text;

namespace EngineWrapper;

public class BidInformation
{
    private bool HasInformation { get; }
    private readonly Dictionary<string, int> minRecords;
    private readonly Dictionary<string, int> maxRecords;
    // ReSharper disable once NotAccessedField.Local
    private readonly List<int> ids;
    private readonly List<bool?> controls;
    private readonly List<int> possibleKeyCards;
    private readonly bool? trumpQueen;
            
    public BidInformation(List<Dictionary<string, string>> records)
    {
        HasInformation = records.Any();
        minRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Min")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Min());
        maxRecords = records.SelectMany(x => x).Where(x => x.Key.StartsWith("Max")).GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => int.Parse(x.Value)).Max());
        ids = records.SelectMany(x => x).Where(x => x.Key == "Id").Select(x => Convert.ToInt32(x.Value)).ToList();
        controls = records.Any() 
            ? [GetHasProperty("SpadeControl"), GetHasProperty("HeartControl"), GetHasProperty("DiamondControl"), GetHasProperty("ClubControl")] 
            : [null, null, null, null];
        possibleKeyCards = records.SelectMany(x => x).Where(x => x.Key == "KeyCards" && !string.IsNullOrWhiteSpace(x.Value)).Select(x => int.Parse(x.Value)).ToList();
        trumpQueen = records.Any() ? GetHasProperty("TrumpQueen") : null;

        bool? GetHasProperty(string fieldName)
        {
            var recordsWithValue = records.SelectMany(x => x).Where(x => x.Key == fieldName && !string.IsNullOrWhiteSpace(x.Value)).ToList();
            var p = !recordsWithValue.Any() ? null : (bool?)(int.Parse(recordsWithValue.FirstOrDefault().Value) == 1);
            return p;
        }
    }

    public string GetInformation()
    {
        return !HasInformation ? "No information" :
            GetMinMaxAsText("Spades") + GetMinMaxAsText("Hearts") + GetMinMaxAsText("Diamonds") + GetMinMaxAsText("Clubs") + GetMinMaxAsText("Hcp") +
            $"{GetControlAsText()}" + $"{GetKeyCardsAsText()}" + $"{GetTrumpQueen()}";

        string GetMinMaxAsText(string suit)
        {
            return minRecords.ContainsKey($"Min{suit}") ? $"\n{suit}: {minRecords[$"Min{suit}"]} - {maxRecords[$"Max{suit}"]}" : "";
        }

        string GetControlAsText()
        {
            if (controls.All(x => x is null))
                return "";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\nControls: ");
            foreach (var suit in Enum.GetValues(typeof(Suit)).Cast<Suit>().Except(new[] { Suit.NoTrump }))
                if (controls[3 - (int)suit].GetValueOrDefault())
                    stringBuilder.Append(suit);
            return stringBuilder.ToString();
        }

        string GetKeyCardsAsText()
        {
            if (!possibleKeyCards.Any())
                return "";
            return $"\nKeyCards: {string.Join(",", possibleKeyCards)}";
        }

        string GetTrumpQueen()
        {
            return trumpQueen.HasValue ? $"\nTrumpQueen: {trumpQueen}" : "";
        }
    }
}