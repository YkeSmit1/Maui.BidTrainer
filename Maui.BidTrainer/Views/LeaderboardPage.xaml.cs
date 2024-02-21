using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views
{
    public partial class LeaderboardPage
    {
        public LeaderboardPage(IEnumerable<Account> accounts)
        {
            InitializeComponent();

            ((LeaderboardViewModel)BindingContext).Accounts = [..accounts];
        }
    }
}