using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views
{
    public partial class ResultsPage
    {
        public ResultsPage(Results results)
        {
            InitializeComponent();
            ((ResultsViewModel)BindingContext).Results = results;
        }
    }
}