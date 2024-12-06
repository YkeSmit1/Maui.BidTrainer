using Maui.BidTrainer.Views;

namespace Maui.BidTrainer
{
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(BidTrainerPage), typeof(BidTrainerPage));
            Routing.RegisterRoute(nameof(LogPage), typeof(LogPage));
            Routing.RegisterRoute(nameof(TheoryPage), typeof(TheoryPage));
        }
    }
}
