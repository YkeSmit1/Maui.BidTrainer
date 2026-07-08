using System.Text.Json;
using Serilog;

namespace Maui.BidTrainer.Services;

public class ResultsService
{
    public Results Results { get; private set; } = new();
    private readonly ILogger logger = IPlatformApplication.Current!.Services.GetService<ILogger>();

    public async Task SaveResultsToFile()
    {
        var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
        await File.WriteAllTextAsync(resultsFile, JsonSerializer.Serialize(Results,
            new JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task UploadResultsAsync()
    {
        var username = Preferences.Get("Username", "");
        if (username == "") return;
        var res = Results.AllResults.Values.SelectMany(x => x.Results.Values).ToList();
        await UpdateOrCreateAccount(username, res.Count, res.Count(x => x.AnsweredCorrectly), res.Sum(x => x.TimeElapsed.Ticks));
        return;

        static async Task UpdateOrCreateAccount(string username, int boardPlayed, int correctBoards, long timeElapsed)
        {
            var account = new Account
            {
                username = username,
                numberOfBoardsPlayed = boardPlayed,
                numberOfCorrectBoards = correctBoards,
                timeElapsed = new TimeSpan(timeElapsed)
            };

            var cosmosDbHelper = DependencyService.Get<ICosmosDbHelper>();
            var user = await cosmosDbHelper.GetAccount(username);
            if (user == null || string.IsNullOrWhiteSpace(user.Value.id))
            {
                account.id = Guid.NewGuid().ToString();
                await cosmosDbHelper.InsertAccount(account);
            }
            else
            {
                account.id = user.Value.id;
                await cosmosDbHelper.UpdateAccount(account);
            }
        }
    }

    public async Task LoadResultsFromFile()
    {
        var resultsFile = Path.Combine(FileSystem.AppDataDirectory, "results.json");
        if (File.Exists(resultsFile))
            try
            {
                Results = JsonSerializer.Deserialize<Results>(await File.ReadAllTextAsync(resultsFile));
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Error when loading results");
            }
    }

    public void AddResult(Result result, int board)
    {
        Results.AddResult(result.Lesson, board, result);
    }

    public void RemoveLessonResults(int lessonNr)
    {
        Results.AllResults.Remove(lessonNr);
    }

    public string GetPercentage(int lesson)
    {
        return Results.AllResults.TryGetValue(lesson, out var value) ? value.Percentage : "";
    }

    public List<int> GetWrongBoards(int lesson)
    {
        return Results.AllResults.TryGetValue(lesson, out var value) ? value.GetWrongBoards() : [];
    }
}