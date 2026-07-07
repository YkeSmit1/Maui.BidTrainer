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
    
    partial void OnIsHintModeChanged(bool value)
    {
        boardService.SetHintMode(value);
        LabelMode = value ? "Hint" : "Bid";
    }    
    
    [ObservableProperty] public partial string LabelMode { get; set; } = "Bid";

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
}