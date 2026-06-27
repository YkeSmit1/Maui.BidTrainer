using System.Globalization;
using Common;
using Maui.BidTrainer.ViewModels;

namespace Maui.BidTrainer.Converters;

public class BidToColorConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if  (value is not BidViewModel bidViewModel)
            return null;
        return bidViewModel.Bid.Suit is Suit.Diamonds or Suit.Hearts ? Colors.Red : null;            
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}