using System.Collections.ObjectModel;
using Common;
using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class AuctionViewModel : BaseViewModel
    {
        private ObservableCollection<string> bids = [];
        public ObservableCollection<string> Bids
        {
            get => bids;
            set => SetProperty(ref bids, value);
        }
    }
}
