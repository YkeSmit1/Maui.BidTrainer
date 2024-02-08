using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class ResultsViewModel : BaseViewModel
    {
        private Results results = new Results();
        public Results Results
        {
            get => results;
            set => SetProperty(ref results, value);
        }
    }
}
