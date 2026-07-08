using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.BidTrainer.Services;
using Maui.BidTrainer.Views;

namespace Maui.BidTrainer.ViewModels;

public partial class BidTrainerViewModel : ObservableObject
{
    private readonly ResultsService resultService =
        Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<ResultsService>()
        ?? throw new InvalidOperationException("ResultsService is not registered.");
    
    private readonly BoardService boardService =
        Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<BoardService>()
        ?? throw new InvalidOperationException("BoardService is not registered.");

    [ObservableProperty] public partial bool IsHintMode { get; set; }
    [ObservableProperty] public partial string Username { get; set; } = Preferences.Get("Username", "");
    [ObservableProperty] public partial int Lesson { get; set; }
    [ObservableProperty] public partial int Board { get; set; }
    [ObservableProperty] public partial string Mode { get; set; } = "Bid";
    [ObservableProperty] public partial string Mistake { get; set; } = "";
    
    partial void OnIsHintModeChanged(bool value)
    {
        boardService.IsInHintMode = value;
        Mode = value ? "Hint" : "Bid";
    }

    [RelayCommand]
    private async Task StartLesson()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(StartPage));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task Results()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(ResultsPage2), new Dictionary<string, object> { ["Results"] = resultService.Results });
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task LeaderBoard()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(LeaderboardPage));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task Settings()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ShowLog()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(LogPage));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ShowReport()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(ResultsPage2), new Dictionary<string, object> { ["Results"] = resultService.Results });
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlert("Error", exception.Message, "OK");
        }
    }
}