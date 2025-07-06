using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public partial class LeaderboardViewModel : ObservableObject
{
    [ObservableProperty]
    public partial  List<Account> Accounts {get; set; } = [];

    public LeaderboardViewModel()
    {
        Task.Run(async () =>
        {
            var lAccounts = await DependencyService.Get<ICosmosDbHelper>().GetAllAccounts();
            Accounts = [..lAccounts.OrderByDescending(x => (double)x.numberOfCorrectBoards / x.numberOfBoardsPlayed)];
        });
    }
}