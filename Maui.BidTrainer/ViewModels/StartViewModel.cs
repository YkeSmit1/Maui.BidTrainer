using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class StartViewModel : ObservableObject
{
    private readonly LessonService lessonService =
        Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<LessonService>()
        ?? throw new InvalidOperationException("LessonService is not registered.");

    public ObservableCollection<LessonViewModel> Lessons { get; set; }

    public async Task LoadLessonsAsync()
    {
        var lessons = await lessonService.GetLessonsAsync();
        Lessons = new ObservableCollection<LessonViewModel>(lessons.Select(x => new LessonViewModel(x)));
        OnPropertyChanged(nameof(Lessons));
    }
    
    [RelayCommand]
    private async Task ContinueWhereLeftOff()
    {
        await Shell.Current.GoToAsync(nameof(BidTrainerPage));
    }
    
}