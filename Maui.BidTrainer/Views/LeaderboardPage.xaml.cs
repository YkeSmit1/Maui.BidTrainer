using System.Collections.ObjectModel;
using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderboardPage
    {
        public LeaderboardPage(IEnumerable<Account> accounts)
        {
            InitializeComponent();
            ((LeaderboardViewModel)BindingContext).Accounts = new ObservableCollection<Account>(accounts);

        }
    }
}