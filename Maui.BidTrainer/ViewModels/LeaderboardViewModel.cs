using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels
{
    public partial class LeaderboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<Account> accounts = [];

        public LeaderboardViewModel()
        {
            Task.Run(async () =>
            {
                var lAccounts = await DependencyService.Get<ICosmosDbHelper>().GetAllAccounts();
                Accounts = [..lAccounts.OrderByDescending(x => (double)x.numberOfCorrectBoards / x.numberOfBoardsPlayed)];
            });
        }
    }
}