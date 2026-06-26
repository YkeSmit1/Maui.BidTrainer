using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class LessonViewModel
{
    private readonly Lesson lesson;

    private int LessonNumber => lesson.LessonNr;
    public string Title => lesson.Content;
    
    public LessonViewModel(Lesson lesson)
    {
        this.lesson = lesson;
    }

    [RelayCommand]
    private async Task StartLesson()
    {
        Preferences.Set("CurrentLesson", LessonNumber);
        Preferences.Set("CurrentBoardIndex", 0);
        await Shell.Current.GoToAsync($"{nameof(TheoryPage)}?Lesson={LessonNumber}");
    }
}