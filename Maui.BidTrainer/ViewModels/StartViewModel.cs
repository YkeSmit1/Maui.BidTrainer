using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class StartViewModel : ObservableObject
{
    public ObservableCollection<LessonViewModel> Lessons { get; set; }

    public async Task LoadLessonsAsync()
    {
        using var reader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("lessons.json"));
        var lessons = JsonSerializer.Deserialize<List<Lesson>>(await reader.ReadToEndAsync());
        Lessons = new ObservableCollection<LessonViewModel>(lessons.Select(x => new LessonViewModel(x)));
        OnPropertyChanged(nameof(Lessons));
    }
    
    [RelayCommand]
    private async Task ContinueWhereLeftOff()
    {
        await Shell.Current.GoToAsync(nameof(BidTrainerPage));
    }
    
}