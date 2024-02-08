using System.Collections.ObjectModel;
using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class LeaderboardViewModel : ObservableObject
    {

        private ObservableCollection<Account> accounts;

        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set => SetProperty(ref accounts, value);
        }
    }
}