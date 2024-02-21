using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class LeaderboardViewModel : ObservableObject
    {
        private List<Account> accounts = [];

        public List<Account> Accounts
        {
            get => accounts;
            set => SetProperty(ref accounts, value);
        }
    }
}