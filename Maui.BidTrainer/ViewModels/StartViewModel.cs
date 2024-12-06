using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public class StartViewModel : ObservableObject
{
    public ObservableCollection<Lesson> Lessons { get; set; }
    public IAsyncRelayCommand<int> StartLessonCommand { get; set; } = new AsyncRelayCommand<int>(ChooseLesson);
    public IAsyncRelayCommand ContinueWhereLeftOffCommand { get; set; } = new AsyncRelayCommand(ContinueWhereLeftOff);

    public async Task LoadLessonsAsync()
    {
        using var reader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        Lessons = JsonSerializer.Deserialize<ObservableCollection<Lesson>>(await reader.ReadToEndAsync());
        OnPropertyChanged(nameof(Lessons));
    }

    private static async Task ChooseLesson(int lessonNr)
    {
        Preferences.Set("CurrentLesson", lessonNr);
        Preferences.Set("CurrentBoardIndex", 0);
        await Shell.Current.GoToAsync($"{nameof(TheoryPage)}?Lesson={lessonNr}");
    }

    private static async Task ContinueWhereLeftOff()
    {
        await Shell.Current.GoToAsync(nameof(BidTrainerPage));
    }
}