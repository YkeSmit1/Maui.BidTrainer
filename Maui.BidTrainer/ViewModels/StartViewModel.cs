using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class StartViewModel : ObservableObject
{
    public ObservableCollection<Lesson> Lessons { get; set; }

    public async Task LoadLessonsAsync()
    {
        using var reader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        Lessons = JsonSerializer.Deserialize<ObservableCollection<Lesson>>(await reader.ReadToEndAsync());
        OnPropertyChanged(nameof(Lessons));
    }
    
    [RelayCommand]
    private static async Task StartLesson(int lessonNr)
    {
        Preferences.Set("CurrentLesson", lessonNr);
        Preferences.Set("CurrentBoardIndex", 0);
        await Shell.Current.GoToAsync($"{nameof(TheoryPage)}?Lesson={lessonNr}");
    }

    [RelayCommand]
    private static async Task ContinueWhereLeftOff()
    {
        await Shell.Current.GoToAsync(nameof(BidTrainerPage));
    }
}