using System.Text;
using System.Text.Json.Serialization;

namespace Maui.BidTrainer;

public class Result
{
    public int Lesson { get; set; }
    public int  Board { get; set; }
    public TimeSpan TimeElapsed { get; set; }
    public bool UsedHint { get; set; }
    public bool AnsweredCorrectly { get; set; } = true;
}
public class Results
{
    public class ResultsPerLesson
    {
        [JsonInclude]
        public Dictionary<int, Result> Results { get; set; } = new();
        public void AddResult(int board, Result result) => Results[board] = result;
        public string Title => GetOverview(Results.ToList());
        public string Percentage => (double)Results.Count(x => x.Value.AnsweredCorrectly) / Results.Count * 100 + "%";
    }

    [JsonInclude]
    public SortedDictionary<int, ResultsPerLesson> AllResults { get; set; } = new();
    public void AddResult(int lesson, int board, Result result)
    {
        if (!AllResults.ContainsKey(lesson))
            AllResults[lesson] = new ResultsPerLesson();
        AllResults[lesson].AddResult(board + 1, result);
    }
    public string Title => GetOverview(AllResults.SelectMany(x => x.Value.Results).ToList());

    private static string GetOverview(List<KeyValuePair<int, Result>> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{results.Count(x => x.Value.AnsweredCorrectly)} out of {results.Count} are correct");
        sb.AppendLine($"Time spent: {new TimeSpan(results.Sum(r => r.Value.TimeElapsed.Ticks)):mm\\:ss} ");
        sb.AppendLine($"Hints used: {results.Count(x => x.Value.UsedHint)}");
        return sb.ToString();
    }
    public List<Result> ResultList => AllResults.SelectMany(x => x.Value.Results.Values).ToList();
}