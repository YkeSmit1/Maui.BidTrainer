using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Views;

public partial class ResultsPage2
{
    public ResultsPage2(Results results)
    {
        InitializeComponent();
        ((ResultsViewModel)BindingContext).Results = results;
    }
}