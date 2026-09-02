using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class PracticeViewModel(LessonService lessonService) : ObservableObject
{
    public ObservableCollection<LessonViewModel> Lessons { get; set; }

    [ObservableProperty] public partial LessonViewModel SelectedLesson { get; set; }

    [ObservableProperty] public partial double NumberOfBoards { get; set; } = 5;

    public async Task LoadLessonsAsync()
    {
        var lessons = await lessonService.GetLessonsAsync();
        Lessons = new ObservableCollection<LessonViewModel>(lessons.Select(x => new LessonViewModel(x)));
        OnPropertyChanged(nameof(Lessons));
    }

    [RelayCommand]
    private async Task Play()
    {
        if (SelectedLesson == null)
            return;

        Preferences.Set("CurrentLesson", SelectedLesson.LessonNumber);
        Preferences.Set("NumberOfBoards", (int)NumberOfBoards);
        await Shell.Current.GoToAsync($"{nameof(BidTrainerPage)}?PracticeMode={true}");
    }
}
