using Maui.BidTrainer.Views;

namespace Maui.BidTrainer;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(BidTrainerPage), typeof(BidTrainerPage));
        Routing.RegisterRoute(nameof(LogPage), typeof(LogPage));
        Routing.RegisterRoute(nameof(TheoryPage), typeof(TheoryPage));
        Routing.RegisterRoute(nameof(ResultsPage2), typeof(ResultsPage2));
        Routing.RegisterRoute(nameof(ResultsPage), typeof(ResultsPage));
        Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));
        Routing.RegisterRoute(nameof(LeaderboardPage), typeof(LeaderboardPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}