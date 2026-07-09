using System.Text.Json;

namespace Maui.BidTrainer.Services;

public class LessonService
{
    private List<Lesson> lessons;

    public async Task<List<Lesson>> GetLessonsAsync()
    {
        if (lessons != null) return 
            lessons;
        using var reader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        lessons = JsonSerializer.Deserialize<List<Lesson>>(await reader.ReadToEndAsync());
        return lessons;
    }}